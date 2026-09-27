using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderFlow.Orders.IntegrationTests;

public sealed class OrdersEndpointsTests(OrdersWebApplicationFactory factory)
    : IClassFixture<OrdersWebApplicationFactory>
{
    [Fact]
    public async Task OrdersEndpoints_ReturnUnauthorized_WhenNoTokenIsProvided()
    {
        var response = await factory.CreateClient().GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CanCreateAnOrder()
    {
        var client = await CreateAuthenticatedCustomerClientAsync();

        var orderId = await CreateOrderAsync(client);

        Assert.NotEqual(Guid.Empty, orderId);
    }

    [Fact]
    public async Task Customer_CanRetrieveTheirOwnOrder()
    {
        var client = await CreateAuthenticatedCustomerClientAsync();
        var orderId = await CreateOrderAsync(client);

        var response = await client.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CanListOnlyTheirOwnOrders()
    {
        var customer = await CreateAuthenticatedCustomerClientAsync();
        var otherCustomer = await CreateAuthenticatedCustomerClientAsync();
        var customerOrderId = await CreateOrderAsync(customer);
        await CreateOrderAsync(otherCustomer);

        var response = await customer.GetAsync("/api/orders");
        var orders = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedOrders = orders.EnumerateArray().ToArray();
        Assert.Single(returnedOrders);
        Assert.Equal(customerOrderId, returnedOrders[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Customer_CannotRetrieveAnotherCustomersOrder()
    {
        var owner = await CreateAuthenticatedCustomerClientAsync();
        var otherCustomer = await CreateAuthenticatedCustomerClientAsync();
        var orderId = await CreateOrderAsync(owner);

        var response = await otherCustomer.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanRetrieveAnotherCustomersOrder()
    {
        var customer = await CreateAuthenticatedCustomerClientAsync();
        var orderId = await CreateOrderAsync(customer);
        var administrator = factory.CreateClient();
        var adminAuthentication = await factory.CreateAdminAsync();
        administrator.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminAuthentication.AccessToken);

        var response = await administrator.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedCustomerClientAsync()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"customer-{Guid.NewGuid():N}@example.test",
            password = "CustomerPass1"
        });
        response.EnsureSuccessStatusCode();
        var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authentication!.AccessToken);
        return client;
    }

    private static async Task<Guid> CreateOrderAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "Demo Customer",
            items = new[]
            {
                new { sku = "KB-001", quantity = 1, unitPrice = 99.00m }
            }
        });
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();

        return order.GetProperty("id").GetGuid();
    }

    private sealed record AuthenticationResponse(string AccessToken);
}
