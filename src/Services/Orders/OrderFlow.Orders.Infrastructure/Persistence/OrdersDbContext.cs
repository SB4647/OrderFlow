using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Infrastructure.Identity;
using OrderFlow.Orders.Domain;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}
