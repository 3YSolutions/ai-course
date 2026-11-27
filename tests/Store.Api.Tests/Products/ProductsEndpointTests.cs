using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Store.Api.Products;

namespace Store.Api.Tests.Products;

public class ProductsEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetProducts_ReturnsHardcodedList()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Product[]>();
        Assert.NotNull(body);
        Assert.Equal(5, body.Length);
        Assert.All(body, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
    }
}
