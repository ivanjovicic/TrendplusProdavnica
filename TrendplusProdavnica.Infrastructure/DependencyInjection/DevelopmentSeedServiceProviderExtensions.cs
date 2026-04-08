#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Seeding;

namespace TrendplusProdavnica.Infrastructure.DependencyInjection
{
    public static class DevelopmentSeedServiceProviderExtensions
    {
        public static async Task SeedDevelopmentDataAsync(
            this IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DevelopmentSeedRunner");
            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();

            logger.LogInformation("Applying migrations before development seed.");
            await db.Database.MigrateAsync(cancellationToken);

            var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
