using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer: EntityBase
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }

        public ICollection<Order> Orders { get;} = new HashSet<Order>();
        public Customer(string email, string name, string phone)
        {
            Email = email;
            Name = name;
            Phone = phone;
        }

    }
}
