using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TrendplusProdavnica.Tests.Integration;

public sealed class AdminAuthorizationIntegrationTests : IClassFixture<TrendplusApiWebApplicationFactory>
{
    private readonly TrendplusApiWebApplicationFactory _factory;

    public AdminAuthorizationIntegrationTests(TrendplusApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AdminAuthMe_WithoutToken_Returns401()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminAuthMe_WithNonAdminRole_Returns403()
    {
        using var client = CreateClient("Customer");

        var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminAuthMe_WithLowercaseAdminRole_Returns200()
    {
        using var client = CreateClient("admin");

        var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminSearchQueueStatus_WithoutToken_Returns401()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/admin/search/queue/status");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminSearchQueueStatus_WithNonAdminRole_Returns403()
    {
        using var client = CreateClient("Customer");

        var response = await client.GetAsync("/api/admin/search/queue/status");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private HttpClient CreateClient(string? role = null)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        if (!string.IsNullOrWhiteSpace(role))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                TestJwtTokenFactory.CreateToken(role));
        }

        return client;
    }
}
