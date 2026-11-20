using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_CreatesProduct_And_GetByLocation_Returns200()
    {
        var client = _factory.CreateClient();
        var sku = $"TEST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var request = new SaveProductRequest(sku, "Тестовий продукт", "шт", 10);

        // Act - POST
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);

        // Act - GET by Location
        var getResponse = await client.GetAsync(location);

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(sku, product.Sku);
    }
}