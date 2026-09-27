using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure.Identity;

public static class OrdersIdentityServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must be configured and at least 32 characters long.");
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<OrdersDbContext>();

        services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
