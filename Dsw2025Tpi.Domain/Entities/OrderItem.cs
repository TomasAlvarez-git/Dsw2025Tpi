using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class OrderItem : EntityBase
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; }

        public Guid OrderId { get; }

        public Guid ProductId { get;}

        public Product? Product { get; set; } // Navigation property

        public Order? Order { get; set; } // Navigation property

        public OrderItem() { } 
        public OrderItem(Guid productId, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

    }

}
