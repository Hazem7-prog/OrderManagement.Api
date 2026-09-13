using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("Orders");

            order.HasKey(x => x.Id);

            order.Property(x => x.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            order.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            order.Property(x => x.RowVersion)
            .IsRowVersion();
        });
    }
}