using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Domain.Entities
{
    // Clase que representa un ítem o línea dentro de una orden
    public class OrderItem : EntityBase
    {
        // Cantidad de unidades del producto en este ítem
        public int Quantity { get; set; }

        // Precio unitario del producto al momento de la compra
        public decimal UnitPrice { get; set; }

        // Subtotal calculado para este ítem (Quantity * UnitPrice)
        public decimal Subtotal { get; set; }

        // Clave foránea que indica a qué orden pertenece este ítem
        public Guid OrderId { get; set; }

        // Clave foránea que indica qué producto es este ítem
        public Guid ProductId { get; set; }

        // Propiedad de navegación para acceder a los datos del producto
        public Product? Product { get; set; }

        // Propiedad de navegación para acceder a la orden que contiene este ítem
        public Order? Order { get; set; }

        // Constructor vacío requerido por EF y para serialización
        public OrderItem() { }

        // Constructor que inicializa un ítem con producto, cantidad y precio unitario
        public OrderItem(Guid productId, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = quantity * unitPrice; // Calcula el subtotal al crear el ítem
        }
    }
}
