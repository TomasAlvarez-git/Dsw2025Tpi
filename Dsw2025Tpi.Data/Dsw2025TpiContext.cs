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
            p.Property(p => p.InternalCode).IsRequired().HasMaxLength(30);
            p.Property(p => p.Description).HasMaxLength(500);
            p.Property(p => p.CurrentPrice).IsRequired().HasPrecision(15, 2);
            p.Property(p => p.StockQuantity).IsRequired();
            p.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<Order>(o =>
        {
            o.HasKey(o => o.Id);
            o.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            o.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            o.Property(o => o.Date).IsRequired();
            o.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(200);
            o.Property(o => o.BillingAddress).IsRequired().HasMaxLength(200);
            o.Property(o => o.Notes).HasMaxLength(500);
            o.Property(o => o.Status).IsRequired();
            o.Property(o => o.TotalAmount).IsRequired().HasPrecision(15, 2);
        });

        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.HasKey(oi => oi.Id);
            oi.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            oi.Property(oi => oi.Quantity).IsRequired();
            oi.Property(oi => oi.UnitPrice).IsRequired().HasPrecision(15, 2);
            oi.Property(oi => oi.Subtotal).HasPrecision(15, 2);
        });

        modelBuilder.Entity<Customer>(c =>
        {
            c.HasKey(c => c.Id);
            c.Property(c => c.Id).IsRequired();
            c.Property(c => c.Name).IsRequired().HasMaxLength(60);
            c.Property(c => c.Email).IsRequired().HasMaxLength(100);
            c.Property(c => c.Phone).HasMaxLength(15);
        });
    }

}
