using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Contracts;
using OrderFlow.Orders.Api.ExceptionHandling;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Infrastructure.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<OrderValidationExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddOrdersPersistence(builder.Configuration);
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddHealthChecks();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IOrderSubmittedPublisher, NoOpOrderSubmittedPublisher>();
}
else
{
    var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
    var rabbitMqUsername = builder.Configuration["RabbitMq:Username"] ?? "orderflow";
    var rabbitMqPassword = builder.Configuration["RabbitMq:Password"] ?? string.Empty;

    builder.Services.AddMassTransit(configuration =>
    {
        configuration.UsingRabbitMq((_, busConfiguration) =>
        {
            busConfiguration.Host(rabbitMqHost, "/", hostConfiguration =>
            {
                hostConfiguration.Username(rabbitMqUsername);
                hostConfiguration.Password(rabbitMqPassword);
            });
        });
    });
    builder.Services.AddScoped<IOrderSubmittedPublisher, MassTransitOrderSubmittedPublisher>();
}

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var order = await ordersService.CreateAsync(
        new CreateOrderCommand(
            request.CustomerName,
            request.Items?.Select(item => new CreateOrderItemCommand(item.Sku, item.Quantity, item.UnitPrice)).ToArray()),
        cancellationToken);

    return Results.CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
});

app.MapGet("/api/orders/{id:guid}", async (
    Guid id,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var order = await ordersService.GetByIdAsync(id, cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
}).WithName("GetOrderById");

app.MapGet("/api/orders", async (
    int? take,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var orders = await ordersService.GetRecentAsync(take ?? 20, cancellationToken);
    return Results.Ok(orders);
});

app.MapHealthChecks("/health");

app.Run();

public partial class Program;

internal sealed class NoOpOrderSubmittedPublisher : IOrderSubmittedPublisher
{
    public Task PublishAsync(OrderFlow.Contracts.OrderSubmitted orderSubmitted, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
