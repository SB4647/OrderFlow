using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Application;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public static class PaymentsPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Payments")
            ?? throw new InvalidOperationException("Connection string 'Payments' is required.");

        services.AddDbContext<PaymentsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPaymentsRepository, EfPaymentsRepository>();

        return services;
    }
}
