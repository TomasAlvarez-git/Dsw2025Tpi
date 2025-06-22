using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order: EntityBase
    {
        public DateTime Date { get; set; }
        public string? ShipingAddress { get; set; }
        public string? BillingAddress { get; set; }
        public string? Notes { get; set; }

        public decimal TotalAmount { get;}

        public OrderStatus Status { get; set; } 

        public ICollection<OrderItem> Items { get; } = new HashSet<OrderItem>();

        public Guid? CustomerId { get; set; }

        public Order(DateTime date, string shipingAddress, string billingAddress, string notes, decimal totalAmount, OrderStatus status)
        {
            Date = date;
            ShipingAddress = shipingAddress;
            BillingAddress = billingAddress;
            Notes = notes;
            TotalAmount = Items.Sum(i => i.Subtotal);
            Status = status;
        }
    }
}
