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
            if (request == null ||
                request.OrderItems == null! ||
                !request.OrderItems.Any() ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress))
            {
                throw new ArgumentException("Datos de la orden inválidos o incompletos.");
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
                var orderItem = new OrderItem(i.ProductId, i.Quantity, i.CurrentUnitPrice);
                return orderItem;
            }).ToList();

            var orderTotal = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            // Crear la orden
            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems);

            await _repository.Add(order);


            // Armar respuesta
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

        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _repository.GetById<Order>(
            id,
            include: new string[] { "Items", "Items.Product" }
        );
        }

    }
}

