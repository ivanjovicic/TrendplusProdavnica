#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Inventory.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Inventory.Workers
{
    #if false
    // InventorySyncWorker and InventorySyncWorkerConfig temporarily disabled due to StoreInventories DbSet missing
    
    /// <summary>
    /// Konfiguracija za inventory sync worker
    /// </summary>
    public class InventorySyncWorkerConfig
    {
        public int IntervalMs { get; set; } = 30000; // 30 sekundi
        public int InitialDelayMs { get; set; } = 5000; // 5 sekundi
        public int BatchSize { get; set; } = 100;
    }

    /// <summary>
    /// Background worker koji sinhronizira inventar sa OpenSearch i invalidira cache
    /// </summary>
    public sealed class InventorySyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly InventorySyncWorkerConfig _config;
        private readonly ILogger<InventorySyncWorker> _logger;

        public InventorySyncWorker(
            IServiceProvider serviceProvider,
            IOptions<InventorySyncWorkerConfig> config,
            ILogger<InventorySyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InventorySyncWorker started");

            // Početna kašnjenja za dozivanje app startup-a
            await Task.Delay(_config.InitialDelayMs, stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.IntervalMs));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        await SyncInventoriesAsync(stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error in inventory sync worker");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InventorySyncWorker stopped");
            }
            finally
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Sinhronizira inventare koji su nedavno promijenjeni
        /// </summary>
        private async Task SyncInventoriesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            try
            {
                // Dohvata inventare koji su promijenjeni u posljednjih N sekundi
                var changedThreshold = DateTimeOffset.UtcNow.AddSeconds(-60);

                var changedVariants = await db.StoreInventories
                    .AsNoTracking()
                    .Where(si => si.UpdatedAtUtc >= changedThreshold)
                    .Select(si => si.VariantId)
                    .Distinct()
                    .Take(_config.BatchSize)
                    .ToListAsync(cancellationToken);

                if (changedVariants.Count == 0)
                {
                    _logger.LogDebug("No inventories to sync");
                    return;
                }

                _logger.LogInformation("Syncing {Count} variant inventories", changedVariants.Count);

                foreach (var variantId in changedVariants)
                {
                    try
                    {
                        // Sinhronizira sa OpenSearch
                        await inventoryService.SyncInventoryToSearchAsync(variantId, cancellationToken);

                        // Invalidira cache
                        await inventoryService.InvalidateCacheAsync(variantId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing inventory for variant {VariantId}", variantId);
                    }
                }

                _logger.LogDebug("Inventory sync completed for {Count} variants", changedVariants.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in inventory sync batch");
            }
        }
    }
    #endif  // InventorySyncWorker disabled due to missing StoreInventories DbSet
}
