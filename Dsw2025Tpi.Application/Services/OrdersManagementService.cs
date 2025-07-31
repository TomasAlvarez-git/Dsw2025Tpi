using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService
    {
        private readonly IRepository _repository;

        // Constructor que recibe un repositorio genérico para operar con la base de datos
        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        // Método para agregar una nueva orden, validando datos, stock y actualizando inventario
        public async Task<OrderModel.Response> AddOrder(OrderModel.Request request)
        {
            // Validación básica de la orden
            if (request == null ||
                request.OrderItems == null ||
                request.OrderItems.Count == 0 ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                throw new BadRequestException("Datos de la orden inválidos o incompletos.");
            }

            // Validar que ningún ProductId sea Guid.Empty para evitar error de diccionario
            if (request.OrderItems.Any(i => i.ProductId == Guid.Empty))
            {
                throw new BadRequestException("Uno o más productos tienen Id vacío.");
            }

            // Obtener productos que existen en la base
            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

            if (productsList == null || productsList.Count() != productIds.Count)
            {
                throw new BadRequestException("Uno o más productos no existen.");
            }

            var products = productsList.ToDictionary(p => p.Id);

            // Crear ítems de orden usando constructor con validaciones internas (cantidad, stock)
            var orderItems = request.OrderItems.Select(i =>
            {
                var product = products[i.ProductId];
                return new OrderItem(product, product.Id, i.Quantity, product.CurrentPrice);
            }).ToList();

            // Descontar stock
            foreach (var item in orderItems)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product);
            }

            // Fecha local Argentina
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            var fechaLocalArgentina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone);

            // Crear orden
            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems)
            {
                Date = fechaLocalArgentina
            };

            await _repository.Add(order);

            // Preparar respuesta
            var response = new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products[oi.ProductId];
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product.Name ?? string.Empty,
                        Description: product.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            );

            return response;
        }



        // Método para obtener una lista paginada de órdenes, filtrando opcionalmente por estado y cliente
        public async Task<List<OrderModel.Response>> GetOrders(OrderStatus? status, Guid? customerId, int pageNumber, int pageSize)
        {
            // Construir filtro dinámico según parámetros recibidos
            Expression<Func<Order, bool>> filter = o =>
                (!status.HasValue || o.Status == status.Value) &&
                (!customerId.HasValue || o.CustomerId == customerId.Value);

            // Traer órdenes de base de datos incluyendo sus ítems
            var allOrders = await _repository.GetFiltered<Order>(filter, "Items");

            if (allOrders == null)
                return new List<OrderModel.Response>();

            // Aplicar paginación manual (skip y take)
            var pagedOrders = allOrders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Obtener todos los IDs de productos de las órdenes paginadas para obtener datos completos
            var productIds = pagedOrders
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

            // Construir la lista de respuestas mapeando cada orden con sus detalles y productos
            var responseList = pagedOrders.Select(order => new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products.GetValueOrDefault(oi.ProductId);
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product?.Name ?? string.Empty,
                        Description: product?.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            )).ToList();

            return responseList;
        }

        // Obtener una orden completa por su ID, incluyendo los ítems y los productos relacionados
        public async Task<Order?> GetOrderById(Guid Id)
        {
            var orders = await _repository.GetFiltered<Order>(
                o => o.Id == Id,
                include: new[] { "Items", "Items.Product" }
            );

            var order = orders.FirstOrDefault();
            if (order == null)
            {
                throw new NotFoundException($"No se encontró la orden con el ID {Id}");
            }

            return order;
        }

        // Actualiza el estado de una orden y devuelve su información actualizada
        public async Task<OrderModel.Response?> UpdateOrderStatus(Guid id, string newStatusText)
        {
            // Traer la orden con los ítems para validar y actualizar
            var order = await _repository.GetById<Order>(id, "Items");

            if (order == null)
                throw new NotFoundException("La orden solicitada no existe");

            // Validar que no sea un número (para evitar casos como "1", "2", etc.)
            if (int.TryParse(newStatusText, out _))
                throw new BadRequestException("No se permite ingresar un número como estado. Usá uno de los siguientes: PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELED.");

            // Validar que el string sea un valor definido del enum (case-insensitive)
            if (!Enum.TryParse<OrderStatus>(newStatusText, true, out var newStatus) ||
                !Enum.GetNames(typeof(OrderStatus)).Contains(newStatus.ToString()))
            {
                throw new BadRequestException("Estado de orden inválido. Debe ser uno de: PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELED.");
            }

            // Solo actualizar si el estado es diferente (idempotencia)
            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                await _repository.Update(order);
            }

            // Obtener productos involucrados para devolver datos completos
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

            // Construir y devolver la respuesta
            return new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products.GetValueOrDefault(oi.ProductId);
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product?.Name ?? string.Empty,
                        Description: product?.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            );
        }


    }
}

