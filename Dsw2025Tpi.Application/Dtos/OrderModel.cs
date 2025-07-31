using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dsw2025Tpi.Application.Dtos
{
    // Modelo de datos para representar una orden de compra
    public record OrderModel
    {
        // DTO utilizado para crear una nueva orden
        public record Request(
            Guid CustomerId,                    // ID del cliente que realiza la orden
            string ShippingAddress,             // Dirección de envío
            string BillingAddress,              // Dirección de facturación
            List<OrderItemModel.Request> OrderItems // Lista de ítems que componen la orden
        );

        // DTO utilizado para actualizar el estado de una orden (por ejemplo, de "Pending" a "Shipped")
        public record UpdateOrderStatusRequest(
            string NewStatus
        );

        // DTO utilizado para devolver una orden al cliente (response)
        public record Response(
            Guid Id,                             // ID de la orden
            Guid CustomerId,                     // ID del cliente
            string? ShippingAddress,             // Dirección de envío (puede ser nula)
            string? BillingAddress,              // Dirección de facturación (puede ser nula)
            DateTime Date,                       // Fecha de creación de la orden
            decimal? TotalAmount,                // Monto total de la orden (puede ser nulo)
            string? Status,                      // Estado actual de la orden (puede ser nulo)
            List<OrderItemModel.Response> OrderItems // Lista de ítems con sus detalles
        );
    }
}

