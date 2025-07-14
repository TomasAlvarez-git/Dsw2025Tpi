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

        public decimal Subtotal => Quantity * UnitPrice;

        public Guid OrderID { get; set; }

        public Guid ProductID { get; set; }

        public OrderItem() { } 
        public OrderItem(Guid productId, int quantity, decimal unitPrice)
        {
            ProductID = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

    }

}
