#nullable enable
using Microsoft.Extensions.DependencyInjection;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Catalog.Services.Implementations;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Content.Services.Implementations;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Application.Stores.Services.Implementations;

namespace TrendplusProdavnica.Application.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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
