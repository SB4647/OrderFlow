using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderFlow.Orders.IntegrationTests;

public sealed class OrdersEndpointsTests(OrdersWebApplicationFactory factory)
    : IClassFixture<OrdersWebApplicationFactory>
{
    [Fact]
    public async Task CreateOrder_ReturnsCreated()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Demo Customer",
            items = new[]
            {
                new { sku = "KB-001", quantity = 1, unitPrice = 99.00m }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", order.GetProperty("status").GetString());
        Assert.NotEqual(Guid.Empty, order.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task GetOrderById_ReturnsCreatedOrder()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Demo Customer",
            items = new[]
            {
                new { sku = "KB-001", quantity = 1, unitPrice = 99.00m }
            }
        });
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createdOrder.GetProperty("id").GetGuid();

        var response = await client.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(orderId, order.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task CreateOrder_ReturnsProblemDetails_WhenItemsAreEmpty()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Demo Customer",
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problemDetails.GetProperty("errors").TryGetProperty("items", out _));
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
