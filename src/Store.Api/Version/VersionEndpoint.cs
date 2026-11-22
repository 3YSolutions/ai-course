using System.Reflection;

namespace Store.Api.Version;

public static class VersionEndpoint
{
    public static IEndpointRouteBuilder MapVersionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/version", () => new VersionResponse(GetAssemblyVersion()))
            .WithName("GetVersion");

        return app;
    }

    private static string GetAssemblyVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
}

public sealed record VersionResponse(string Version);
