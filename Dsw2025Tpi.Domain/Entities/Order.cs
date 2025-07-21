using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsw2025Tpi.Domain.Entities
{
    // Clase que representa una orden de compra
    public class Order : EntityBase
    {
        // Fecha en que se realiza la orden
        public DateTime Date { get; set; }

        // Dirección de envío para la orden
        public string? ShippingAddress { get; set; }

        // Dirección de facturación para la orden
        public string? BillingAddress { get; set; }

        // Notas adicionales que el cliente o el sistema quiera agregar
        public string? Notes { get; set; }

        // Monto total de la orden, calculado con base en los ítems
        public decimal TotalAmount { get; set; }

        // Estado actual de la orden (por ejemplo, pendiente, completada, cancelada)
        public OrderStatus Status { get; set; }

        // Colección de ítems que forman parte de esta orden
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        // Clave foránea opcional que relaciona la orden con un cliente
        public Guid? CustomerId { get; set; }

        // Propiedad de navegación para acceder al cliente relacionado
        public Customer? Customer { get; set; }

        // Constructor por defecto (necesario para EF y serialización)
        public Order() { }

        // Constructor que inicializa una nueva orden con datos esenciales
        public Order(Guid Customerid, string shippingAddress, string billingAddress, ICollection<OrderItem> items)
        {
            CustomerId = Customerid;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Items = items;
            Date = DateTime.UtcNow;  // Fecha actual en formato UTC
            Status = OrderStatus.PENDING;  // Estado inicial por defecto
            TotalAmount = items.Sum(i => i.Subtotal);  // Cálculo del total sumando los subtotales de los ítems
        }
    }
}
