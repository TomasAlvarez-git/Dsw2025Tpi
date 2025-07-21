using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
            // Validación básica de que los datos no sean nulos o vacíos
            if (request == null ||
                request.OrderItems == null! ||
                !request.OrderItems.Any() ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                throw new ArgumentException("Datos de la orden inválidos o incompletos.");
            }

            // Extraer IDs únicos de productos de la orden
            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();

            // Traer todos los productos involucrados en la orden desde la base de datos
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

            // Validar que todos los productos solicitados existan
            if (productsList == null || productsList.Count() != productIds.Count)
            {
                throw new InvalidOperationException("Uno o más productos no existen.");
            }

            // Convertir lista de productos en diccionario para acceso rápido por ID
            var products = productsList.ToDictionary(p => p.Id);

            // Verificar que haya stock suficiente para cada producto solicitado
            foreach (var item in request.OrderItems)
            {
                var product = products[item.ProductId];
                if (!product.StockQuantity.HasValue || item.Quantity > product.StockQuantity.Value)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto '{product.Name}' (ID: {product.Id}). Disponible: {product.StockQuantity ?? 0}, solicitado: {item.Quantity}.");
                }
            }

            // Descontar la cantidad pedida de stock para cada producto y actualizar en base de datos
            foreach (var item in request.OrderItems)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product); // Guardar cambios para cada producto
            }

            // Crear los ítems de la orden con los datos necesarios (Producto, cantidad, precio)
            var orderItems = request.OrderItems.Select(i =>
            {
                var orderItem = new OrderItem(i.ProductId, i.Quantity, i.CurrentUnitPrice);
                return orderItem;
            }).ToList();

            // Calcular el total de la orden sumando el subtotal de cada ítem
            var orderTotal = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            // Crear la entidad orden con cliente, direcciones y los ítems
            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems);

            // Agregar la orden al repositorio (persistencia)
            await _repository.Add(order);

            // Construir y devolver el objeto de respuesta con los datos de la orden creada
            var response = new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date.Date,
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
        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _repository.GetById<Order>(
            id,
            include: new string[] { "Items", "Items.Product" } // carga relacionada para evitar lazy loading
            );
        }

        // Actualiza el estado de una orden y devuelve su información actualizada
        public async Task<OrderModel.Response?> UpdateOrderStatus(Guid id, OrderStatus newStatus)
        {
            // Traer la orden con los ítems para validar y actualizar
            var order = await _repository.GetById<Order>(id, "Items");

            if (order == null)
                return null;

            // Validar que el nuevo estado sea válido en el enum
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                throw new ArgumentException("Estado de orden inválido.");

            // Validar que el estado no sea un estado intermedio no permitido
            if ((int)newStatus < 1 || (int)newStatus > 5)
                throw new ArgumentException("El estado de la orden no puede ser un estado intermedio.");

            // Solo actualizar si el estado es diferente (idempotencia)
            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                await _repository.Update(order);
            }

            // Obtener productos involucrados para devolver datos completos
            var productIds = order.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

            // Construir y devolver la respuesta con la orden actualizada
            return new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date.Date,
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

