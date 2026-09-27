using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInventoryPersistence(builder.Configuration);
builder.Services.AddScoped<IInventoryService, InventoryService>();

var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMq:Username"] ?? "orderflow";
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"] ?? string.Empty;

builder.Services.AddMassTransit(configuration =>
{
    configuration.AddConsumer<OrderSubmittedConsumer>();
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
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await dbContext.Database.MigrateAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
}

await host.RunAsync();
