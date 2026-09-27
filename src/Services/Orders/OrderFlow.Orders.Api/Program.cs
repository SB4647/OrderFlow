using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Orders.Api.Contracts;
using OrderFlow.Orders.Api.ExceptionHandling;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Infrastructure.Messaging;
using OrderFlow.Orders.Infrastructure.Identity;
using OrderFlow.Orders.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<OrderValidationExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddOrdersPersistence(builder.Configuration);
builder.Services.AddOrdersIdentity(builder.Configuration);
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddHealthChecks();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => policy.RequireRole(Roles.Customer));
});

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
        configuration.AddConsumer<PaymentSucceededConsumer>();
        configuration.AddConsumer<PaymentFailedConsumer>();
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
    builder.Services.AddScoped<IOrderSubmittedPublisher, MassTransitOrderSubmittedPublisher>();
}

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }

    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>()
        .SeedRolesAsync(app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    ClaimsPrincipal user,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var order = await ordersService.CreateAsync(
        new CreateOrderCommand(
            GetUserId(user),
            request.CustomerName,
            request.Items?.Select(item => new CreateOrderItemCommand(item.Sku, item.Quantity, item.UnitPrice)).ToArray()),
        cancellationToken);

    return Results.CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
}).RequireAuthorization("Customer");

app.MapGet("/api/orders/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var order = await ordersService.GetByIdAsync(id, GetOrderAccessScope(user), cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
}).WithName("GetOrderById").RequireAuthorization();

app.MapGet("/api/orders", async (
    int? take,
    ClaimsPrincipal user,
    IOrdersService ordersService,
    CancellationToken cancellationToken) =>
{
    var orders = await ordersService.GetRecentAsync(take ?? 20, GetOrderAccessScope(user), cancellationToken);
    return Results.Ok(orders);
}).RequireAuthorization();

app.MapPost("/api/auth/register", async (
    RegisterRequest request,
    IAuthenticationService authenticationService,
    CancellationToken cancellationToken) =>
{
    var result = await authenticationService.RegisterAsync(
        new RegisterUserCommand(request.Email, request.Password),
        cancellationToken);
    return result is null
        ? Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Registration failed.")
        : Results.Created("/api/auth/me", result);
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    IAuthenticationService authenticationService,
    CancellationToken cancellationToken) =>
{
    var result = await authenticationService.LoginAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
    return result is null
        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.")
        : Results.Ok(result);
}).AllowAnonymous();

app.MapGet("/api/auth/me", async (
    ClaimsPrincipal user,
    IAuthenticationService authenticationService,
    CancellationToken cancellationToken) =>
{
    var currentUser = await authenticationService.GetCurrentUserAsync(GetUserId(user), cancellationToken);
    return currentUser is null ? Results.Unauthorized() : Results.Ok(currentUser);
}).RequireAuthorization();

app.MapHealthChecks("/health");

app.Run();

static Guid GetUserId(ClaimsPrincipal user) =>
    Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

static OrderAccessScope GetOrderAccessScope(ClaimsPrincipal user) =>
    new(GetUserId(user), user.IsInRole(Roles.Admin));

public partial class Program;

internal sealed class NoOpOrderSubmittedPublisher : IOrderSubmittedPublisher
{
    public Task PublishAsync(OrderFlow.Contracts.OrderSubmitted orderSubmitted, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
