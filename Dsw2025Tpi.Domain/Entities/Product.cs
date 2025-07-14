using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public string? Sku { get; set; }
        public string? InternalCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public decimal CurrentPrice { get; set; }

        public int? StockQuantity { get; set; } 

        public bool IsActive { get; set; }

        public ICollection<OrderItem> OrderItems { get; } = new HashSet<OrderItem>();

        public Product() { }
        public Product(string sku, string internalCode, string name, string description, decimal currentPrice, int stockQuantity)
        {
            Sku = sku;
            InternalCode = internalCode;
            Name = name;
            Description = description;
            CurrentPrice = currentPrice;
            StockQuantity = stockQuantity;
            IsActive = true;
        }


    }
}
