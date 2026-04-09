#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Search.Services;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class ProductSearchReindexHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SearchSettings _settings;
        private readonly ILogger<ProductSearchReindexHostedService> _logger;

        public ProductSearchReindexHostedService(
            IServiceProvider serviceProvider,
            IOptions<SearchSettings> settings,
            ILogger<ProductSearchReindexHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_settings.RunReindexOnStartup)
            {
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var indexService = scope.ServiceProvider.GetRequiredService<IProductSearchIndexService>();
                await indexService.ReindexAllAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup product reindex failed.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
