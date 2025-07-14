using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService
    {
        private readonly IRepository _repository;

        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrderModel.Response> AddOrder(OrderModel.Request request)
        {
            if (request.OrderItems == null || !request.OrderItems.Any())
            {
                throw new ArgumentException("La orden debe contener al menos un producto.");
            }

            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();

            // Traer todos los productos involucrados en la orden
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));

            if (productsList == null || productsList.Count() != productIds.Count)
            {
                throw new InvalidOperationException("Uno o más productos no existen.");
            }

            var products = productsList.ToDictionary(p => p.Id);

            // Verificar stock
            foreach (var item in request.OrderItems)
            {
                var product = products[item.ProductId];
                if (!product.StockQuantity.HasValue || item.Quantity > product.StockQuantity.Value)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto '{product.Name}' (ID: {product.Id}). Disponible: {product.StockQuantity ?? 0}, solicitado: {item.Quantity}.");
                }
            }

            // Descontar stock y actualizar productos
            foreach (var item in request.OrderItems)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product); // guardar cambios por producto
            }

            // Crear los ítems de la orden con ProductID asignado
            var orderItems = request.OrderItems.Select(i =>
            {
                var orderItem = new OrderItem(i.ProductId, i.Quantity, i.CurrentUnitPrice)
                {
                    ProductID = i.ProductId
                };
                return orderItem;
            }).ToList();

            // Crear la orden
            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems);

            await _repository.Add(order);

            // Armar respuesta
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
                    var product = products[oi.ProductID];
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductID,
                        Name: product.Name ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            );

            return response;
        }


    }
}
