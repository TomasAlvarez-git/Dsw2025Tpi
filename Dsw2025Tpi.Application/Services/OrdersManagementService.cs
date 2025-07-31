using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OrdersManagementService> _logger;

        public OrdersManagementService(IRepository repository, ILogger<OrdersManagementService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<OrderModel.Response> AddOrder(OrderModel.Request request)
        {
            _logger.LogInformation("Iniciando creación de orden para cliente {CustomerId}", request.CustomerId);

            if (request == null ||
                request.OrderItems == null ||
                request.OrderItems.Count == 0 ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                _logger.LogWarning("Datos incompletos o inválidos para la orden: {@Request}", request);
                throw new BadRequestException("Datos de la orden inválidos o incompletos.");
            }

            if (request.OrderItems.Any(i => i.ProductId == Guid.Empty))
            {
                _logger.LogWarning("Se encontró uno o más ProductId vacíos en la orden.");
                throw new BadRequestException("Uno o más productos tienen Id vacío.");
            }

            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

            if (productsList == null || productsList.Count() != productIds.Count)
            {
                _logger.LogWarning("La orden contiene uno o más productos inexistentes.");
                throw new BadRequestException("Uno o más productos no existen.");
            }

            var products = productsList.ToDictionary(p => p.Id);

            var orderItems = request.OrderItems.Select(i =>
            {
                var product = products[i.ProductId];
                return new OrderItem(product, product.Id, i.Quantity, product.CurrentPrice);
            }).ToList();

            foreach (var item in orderItems)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product);
            }

            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            var fechaLocalArgentina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone);

            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems)
            {
                Date = fechaLocalArgentina
            };

            await _repository.Add(order);

            _logger.LogInformation("Orden creada exitosamente con ID: {OrderId}", order.Id);

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

        public async Task<List<OrderModel.Response>> GetOrders(OrderStatus? status, Guid? customerId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Obteniendo órdenes. Filtros - Estado: {Status}, Cliente: {CustomerId}, Página: {Page}, Tamaño: {Size}",
                status?.ToString() ?? "Todos", customerId?.ToString() ?? "Todos", pageNumber, pageSize);

            Expression<Func<Order, bool>> filter = o =>
                (!status.HasValue || o.Status == status.Value) &&
                (!customerId.HasValue || o.CustomerId == customerId.Value);

            var allOrders = await _repository.GetFiltered<Order>(filter, "Items");

            if (allOrders == null || !allOrders.Any())
            {
                _logger.LogInformation("No se encontraron órdenes con los filtros aplicados.");
                return new List<OrderModel.Response>();
            }

            var pagedOrders = allOrders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Se encontraron {Count} órdenes en la página {Page}", pagedOrders.Count, pageNumber);

            var productIds = pagedOrders
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

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

        public async Task<Order?> GetOrderById(Guid Id)
        {
            _logger.LogInformation("Buscando orden por ID: {Id}", Id);

            var orders = await _repository.GetFiltered<Order>(
                o => o.Id == Id,
                include: new[] { "Items", "Items.Product" }
            );

            var order = orders.FirstOrDefault();

            if (order == null)
            {
                _logger.LogWarning("No se encontró ninguna orden con el ID: {Id}", Id);
                throw new NotFoundException($"No se encontró la orden con el ID {Id}");
            }

            _logger.LogInformation("Orden encontrada con ID: {Id}", Id);
            return order;
        }

        public async Task<OrderModel.Response?> UpdateOrderStatus(Guid id, string newStatusText)
        {
            _logger.LogInformation("Actualizando estado de la orden {Id} a '{NewStatus}'", id, newStatusText);

            var order = await _repository.GetById<Order>(id, "Items");
            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden con ID: {Id} para actualizar estado", id);
                throw new NotFoundException("La orden solicitada no existe");
            }

            if (int.TryParse(newStatusText, out _))
            {
                _logger.LogWarning("Estado inválido (numérico) para la orden: '{NewStatus}'", newStatusText);
                throw new BadRequestException("No se permite ingresar un número como estado. Usá uno válido.");
            }

            if (!Enum.TryParse<OrderStatus>(newStatusText, true, out var newStatus) ||
                !Enum.GetNames(typeof(OrderStatus)).Contains(newStatus.ToString()))
            {
                _logger.LogWarning("Estado inválido: '{NewStatus}' no es parte del enum OrderStatus", newStatusText);
                throw new BadRequestException("Estado de orden inválido. Debe ser uno válido.");
            }

            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                await _repository.Update(order);
                _logger.LogInformation("Estado de orden actualizado correctamente a: {NewStatus}", newStatus);
            }
            else
            {
                _logger.LogInformation("El estado de la orden ya es: {NewStatus}, no se realizaron cambios.", newStatus);
            }

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

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

