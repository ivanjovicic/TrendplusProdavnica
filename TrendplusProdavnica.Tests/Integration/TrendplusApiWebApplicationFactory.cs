using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace TrendplusProdavnica.Tests.Integration;

public sealed class TrendplusApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string JwtSecret = "your-very-secret-key-change-this-in-production-at-least-32-characters-long!";
    public const string JwtIssuer = "TrendplusProdavnica";
    public const string JwtAudience = "TrendplusProdavnica.Admin";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TrendplusDb"] = "Host=localhost;Port=5432;Database=trendplus_prodavnica_tests;Username=postgres;Password=postgres",
                ["ConnectionStrings:redis"] = "localhost:6379",
                ["Jwt:SecretKey"] = JwtSecret,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["AdminAuth:Email"] = "admin@trendplus.com",
                ["AdminAuth:Password"] = "admin123!@#",
                ["Search:RunReindexOnStartup"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
        });
    }
}
