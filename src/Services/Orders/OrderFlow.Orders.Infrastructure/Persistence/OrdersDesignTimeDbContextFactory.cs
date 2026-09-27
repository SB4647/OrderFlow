using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public sealed class OrdersDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=orderflow_orders;Username=orderflow")
            .Options;

        return new OrdersDbContext(options);
    }
}
