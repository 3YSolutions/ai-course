namespace Store.Api.Products;

public static class ProductsEndpoint
{
    private static readonly Product[] Products =
    [
        new(1, "Laptop Pro 14", 1499.99m),
        new(2, "Wireless Mouse", 24.90m),
        new(3, "Mechanical Keyboard", 89.50m),
        new(4, "USB-C Hub", 39.00m),
        new(5, "27\" Monitor", 329.00m),
    ];

    public static IEndpointRouteBuilder MapProductsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", () => Products)
            .WithName("GetProducts");

        return app;
    }
}

public sealed record Product(int Id, string Name, decimal Price);
