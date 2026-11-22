using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Store.Api.Version;

namespace Store.Api.Tests.Version;

public class VersionEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetVersion_ReturnsAssemblyVersion()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VersionResponse>();
        Assert.Equal(typeof(Program).Assembly.GetName().Version!.ToString(), body!.Version);
    }
}
