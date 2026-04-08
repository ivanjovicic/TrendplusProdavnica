#nullable enable
using Microsoft.Extensions.DependencyInjection;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Content;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Stores;

namespace TrendplusProdavnica.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureQueries(this IServiceCollection services)
        {
            services.AddScoped<IHomePageQueryService, HomePageQueryService>();
            services.AddScoped<IProductListingQueryService, ProductListingQueryService>();
            services.AddScoped<IProductDetailQueryService, ProductDetailQueryService>();
            services.AddScoped<IBrandPageQueryService, BrandPageQueryService>();
            services.AddScoped<ICollectionPageQueryService, CollectionPageQueryService>();
            services.AddScoped<IEditorialQueryService, EditorialQueryService>();
            services.AddScoped<IStoreQueryService, StoreQueryService>();

            return services;
        }
    }
}
