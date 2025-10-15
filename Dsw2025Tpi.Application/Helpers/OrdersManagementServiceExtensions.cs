using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Helpers
{
    public class OrdersManagementServiceExtensions
    {
        private readonly IRepository _repository;
        private readonly ILogger<OrdersManagementServiceExtensions> _logger;

        public OrdersManagementServiceExtensions(IRepository repository, ILogger<OrdersManagementServiceExtensions> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void ValidateOrderRequest(OrderModel.Request request)
        {
            if (request == null ||
            request.OrderItems == null ||
            request.OrderItems.Count == 0 ||
            string.IsNullOrWhiteSpace(request.ShippingAddress) ||
            string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                _logger.LogWarning("Datos incompletos o inválidos para la orden: {@Request}", request);
                throw new BadRequestException("Datos de la orden inválidos o incompletos.");
            }
        }

        public void ValidateEmptyProducts(OrderModel.Request request)
        {
            if (request.OrderItems.Any(i => i.ProductId == Guid.Empty))
            {
                _logger.LogWarning("Se encontró uno o más ProductId vacíos en la orden.");
                throw new BadRequestException("Uno o más productos tienen Id vacío.");
            }
        }

        public async Task ValidateIdCustomerAsync(OrderModel.Request request)
        {
            if (request.CustomerId == Guid.Empty)
            {
                _logger.LogWarning("CustomerId vacío recibido en la solicitud.");
                throw new BadRequestException("El ID del cliente no puede estar vacío.");
            }

            var existingCustomer = await _repository.GetById<Customer>(request.CustomerId);

            if (existingCustomer == null)
            {
                _logger.LogWarning("Cliente no encontrado con ID: {CustomerId}", request.CustomerId);
                throw new BadRequestException($"No se encontró un cliente con el ID: {request.CustomerId}.");
            }
        }

        public async Task<IEnumerable<Product>> ValidateProductsInListAsync(OrderModel.Request request)
        {
            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

            if (productsList == null || productsList.Count() != productIds.Count)
            {
                _logger.LogWarning("La orden contiene uno o más productos inexistentes.");
                throw new BadRequestException("Uno o más productos no existen.");
            }
            else return productsList;
        }

        public DateTime GetDateArgentinean()
        {
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            var fechaLocalArgentina = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone);

            return fechaLocalArgentina;
        }

        public Order ValidateOrderNull(Guid Id, IEnumerable<Order>? orders)
        {
            var order = orders.FirstOrDefault();
            if (order == null)
            {
                _logger.LogWarning("No se encontró ninguna orden con el ID: {Id}", Id);
                throw new NotFoundException($"No se encontró la orden con el ID {Id}");
            }
            else
            {
                return order;
            }
        }

        //public async Task ValidateAddressesDoNotExistAsync(OrderModel.Request request)
        //{
        //    var matchingOrders = await _repository.GetFiltered<Order>(o =>
        //        o.ShippingAddress.ToLower().Trim() == request.ShippingAddress.ToLower().Trim() ||
        //        o.BillingAddress.ToLower().Trim() == request.BillingAddress.ToLower().Trim());

        //    if (matchingOrders?.Any() == true)
        //    {
        //        _logger.LogWarning("Ya existen órdenes con direcciones iguales.");
        //        throw new BadRequestException("Las direcciones de envío o facturación ya existen en otras órdenes.");
        //    }
        //}

        public async Task ValidateStockAvailabilityAsync(OrderModel.Request request)
        {
            foreach (var item in request.OrderItems)
            {
                var product = await _repository.GetById<Product>(item.ProductId);

                if (product == null) {
                    throw new BadRequestException($"El producto con ID '{item.ProductId}' no existe.");
                }

                if (item.Quantity > product.StockQuantity)
                {
                    _logger.LogWarning("Stock insuficiente para el producto {ProductId}. Solicitado: {Requested}, Disponible: {Available}",
                        item.ProductId, item.Quantity, product.StockQuantity);

                    throw new BadRequestException($"El producto '{product.Name}' no tiene stock suficiente. " +
                        $"Solicitado: {item.Quantity}, Disponible: {product.StockQuantity}.");
                }
            }

            _logger.LogInformation("Validación de stock superada correctamente para la orden del cliente {CustomerId}", request.CustomerId);
        }

    }
}
