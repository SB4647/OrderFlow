using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public static class OrdersPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Orders")
            ?? throw new InvalidOperationException("Connection string 'Orders' is required.");

        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrdersRepository, EfOrdersRepository>();

        return services;
    }
}
