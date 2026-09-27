using OrderFlow.Orders.Api.Contracts;
using OrderFlow.Orders.Api.ExceptionHandling;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<OrderValidationExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddOrdersPersistence(builder.Configuration);
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

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
