using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext : DbContext
{


    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(p =>
        {
            p.HasKey(p => p.Id);
            p.Property(p => p.Sku).IsRequired().HasMaxLength(15);
            p.Property(p => p.Name).IsRequired().HasMaxLength(60);
            p.Property(p => p.InternalCode).HasMaxLength(30);
            p.Property(p => p.Description).HasMaxLength(500);
            p.Property(p => p.CurrentPrice).IsRequired();
            p.Property(p => p.StockQuantity).HasPrecision(15, 2).IsRequired();
            p.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<Order>(o =>
        {
            o.HasKey(o => o.Id);
            o.Property(o => o.Date).IsRequired();
            o.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(200);
            o.Property(o => o.BillingAddress).IsRequired().HasMaxLength(200);
            o.Property(o => o.Notes).HasMaxLength(500);
            o.Property(o => o.TotalAmount).IsRequired().HasPrecision(15, 2);
        });

        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.HasKey(oi => oi.Id);
            oi.Property(oi => oi.Quantity).IsRequired();
            oi.Property(oi => oi.UnitPrice).IsRequired().HasPrecision(15, 2);
        });

        modelBuilder.Entity<Customer>(c =>
        {
            c.HasKey(c => c.Id);
            c.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
            c.Property(c => c.Name).IsRequired().HasMaxLength(60);
            c.Property(c => c.Email).IsRequired().HasMaxLength(100);
            c.Property(c => c.Phone).HasMaxLength(15);
        });
    }

}
