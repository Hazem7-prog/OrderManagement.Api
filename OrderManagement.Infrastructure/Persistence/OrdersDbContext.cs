using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Orders;
using OrderManagement.Infrastructure.Persistence.ReadModels;

namespace OrderManagement.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderDashboardRow> OrderDashboard => Set<OrderDashboardRow>();

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

            order.Property(x => x.Total)
                .HasPrecision(18, 2);

            order.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            order.Navigation(x => x.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(item =>
        {
            item.ToTable("OrderItems");

            item.HasKey(x => x.Id);

            item.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            item.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            item.Ignore(x => x.LineTotal);
        });

        modelBuilder.Entity<OrderDashboardRow>(row =>
        {
            row.ToTable("OrderDashboard");

            row.HasKey(x => x.OrderId);

            row.Property(x => x.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            row.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired();

            row.Property(x => x.Total)
                .HasPrecision(18, 2);
        });
    }
}
