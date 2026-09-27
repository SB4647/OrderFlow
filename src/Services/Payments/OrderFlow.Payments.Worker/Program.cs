using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Infrastructure.Messaging;
using OrderFlow.Payments.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPaymentsPersistence(builder.Configuration);
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentOutcomePublisher, MassTransitPaymentOutcomePublisher>();

var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMq:Username"] ?? "orderflow";
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"] ?? string.Empty;

builder.Services.AddMassTransit(configuration =>
{
    configuration.AddConsumer<InventoryReservedConsumer>();
    configuration.UsingRabbitMq((context, busConfiguration) =>
    {
        busConfiguration.Host(rabbitMqHost, "/", hostConfiguration =>
        {
            hostConfiguration.Username(rabbitMqUsername);
            hostConfiguration.Password(rabbitMqPassword);
        });
        busConfiguration.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    await dbContext.Database.MigrateAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
}

await host.RunAsync();
