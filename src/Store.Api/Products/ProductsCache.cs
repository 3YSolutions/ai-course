using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Store.Api.Products;

public static class ProductsCache
{
    public const string PolicyName = "Products";
    public const string Tag = "products";
    public const string FlagKey = "Features:ProductsCache";
    public static readonly TimeSpan Expiration = TimeSpan.FromMinutes(30);

    public static bool IsEnabled(IConfiguration configuration) => configuration.GetValue(FlagKey, true);

    public static IServiceCollection AddProductsCache(this IServiceCollection services) =>
        services.AddOutputCache(options => options.AddPolicy(PolicyName, new ProductsCachePolicy()));
}

// The flag is read per request so configuration overrides apply without rebuilding the pipeline.
public sealed class ProductsCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var request = context.HttpContext.Request;
        bool enabled = ProductsCache.IsEnabled(configuration)
            && (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method));

        context.EnableOutputCaching = enabled;
        if (!enabled)
        {
            return ValueTask.CompletedTask;
        }

        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = ProductsCache.Expiration;
        context.Tags.Add(ProductsCache.Tag);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;
        if (!StringValues.IsNullOrEmpty(response.Headers[HeaderNames.SetCookie]) || response.StatusCode != StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
