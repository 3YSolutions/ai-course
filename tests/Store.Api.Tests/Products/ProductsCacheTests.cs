using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Api.Products;

namespace Store.Api.Tests.Products;

public class ProductsCacheTests
{
    private static WebApplicationFactory<Program> CreateFactory(bool? flag = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            if (flag is { } value)
            {
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        [ProductsCache.FlagKey] = value.ToString(),
                    }));
            }
        });

    private static async Task AssertFiveProducts(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Product[]>();
        Assert.NotNull(body);
        Assert.Equal(5, body.Length);
    }

    [Fact]
    public async Task GetProducts_FlagOn_SecondRequestServedFromCache()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();

        var first = await client.GetAsync("/products");
        var second = await client.GetAsync("/products");

        await AssertFiveProducts(first);
        await AssertFiveProducts(second);
        Assert.Equal(await first.Content.ReadAsStringAsync(), await second.Content.ReadAsStringAsync());
        Assert.True(second.Headers.Contains("Age"));
    }

    [Fact]
    public async Task GetProducts_FlagOff_SecondRequestNotServedFromCache()
    {
        using var factory = CreateFactory(false);
        var client = factory.CreateClient();

        await client.GetAsync("/products");
        var second = await client.GetAsync("/products");

        await AssertFiveProducts(second);
        Assert.False(second.Headers.Contains("Age"));
    }

    [Fact]
    public async Task ProductsPolicy_ExpiresAfter30Minutes()
    {
        using var factory = CreateFactory();
        var policy = new ProductsCachePolicy();
        var context = new DefaultHttpContext { RequestServices = factory.Services };
        context.Request.Method = HttpMethods.Get;
        var cacheContext = (OutputCacheContext)Activator.CreateInstance(typeof(OutputCacheContext), nonPublic: true)!;
        cacheContext.GetType().GetProperty(nameof(OutputCacheContext.HttpContext))!.SetValue(cacheContext, context);

        await policy.CacheRequestAsync(cacheContext, CancellationToken.None);

        Assert.Equal(TimeSpan.FromMinutes(30), cacheContext.ResponseExpirationTimeSpan);
        Assert.Contains(ProductsCache.Tag, cacheContext.Tags);
    }

    [Fact]
    public async Task DeleteProductsCache_FlagOn_EvictsCache()
    {
        using var factory = CreateFactory();
        var client = factory.CreateClient();
        await client.GetAsync("/products");
        Assert.True((await client.GetAsync("/products")).Headers.Contains("Age"));

        var delete = await client.DeleteAsync("/products/cache");
        var after = await client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await AssertFiveProducts(after);
        Assert.False(after.Headers.Contains("Age"));
    }

    [Fact]
    public async Task DeleteProductsCache_FlagOff_ReturnsNoContent()
    {
        using var factory = CreateFactory(false);
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/products/cache");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
