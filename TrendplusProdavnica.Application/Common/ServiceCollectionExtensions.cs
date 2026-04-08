#nullable enable
using Microsoft.Extensions.DependencyInjection;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Stores.Services;

namespace TrendplusProdavnica.Application.Common
{
    /// <summary>
    /// Extension methods for Application layer DI registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all Application layer services
        /// </summary>
        /// <remarks>
        /// This method registers query service interfaces.
        /// Implementations are registered in Infrastructure layer.
        /// </remarks>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Catalog services
            services.AddScoped<IHomePageQueryService>();
            services.AddScoped<IProductListingQueryService>();
            services.AddScoped<IProductDetailQueryService>();

            // Content services
            services.AddScoped<IBrandPageQueryService>();
            services.AddScoped<ICollectionPageQueryService>();
            services.AddScoped<IEditorialQueryService>();

            // Store services
            services.AddScoped<IStoreQueryService>();

            return services;
        }
    }
}
