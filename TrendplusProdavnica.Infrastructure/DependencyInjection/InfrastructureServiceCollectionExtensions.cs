#nullable enable
using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Application.Wishlist.Services;
using TrendplusProdavnica.Infrastructure.Admin.Services;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Seeding;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Content;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Stores;
using TrendplusProdavnica.Infrastructure.Search;
using TrendplusProdavnica.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructurePerformance(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CacheSettings>(configuration.GetSection("Cache"));
            services.Configure<RedisSettings>(configuration.GetSection("Redis"));
            services.Configure<OpenSearchSettings>(configuration.GetSection("OpenSearch"));
            services.Configure<SearchSettings>(configuration.GetSection("Search"));

            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();
            var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>() ?? new RedisSettings();
            var redisConnectionString = ResolveRedisConnectionString(configuration, redisSettings);

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = redisSettings.InstanceName;
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var fusionBuilder = services
                .AddFusionCache()
                .WithDefaultEntryOptions(new FusionCacheEntryOptions
                {
                    Duration = cacheSettings.Durations.ProductDetail,
                    IsFailSafeEnabled = cacheSettings.IsFailSafeEnabled,
                    FailSafeMaxDuration = cacheSettings.FailSafeMaxDuration,
                    FailSafeThrottleDuration = cacheSettings.FailSafeThrottleDuration,
                    FactorySoftTimeout = cacheSettings.FactorySoftTimeout,
                    FactoryHardTimeout = cacheSettings.FactoryHardTimeout,
                    DistributedCacheDuration = cacheSettings.Durations.ProductDetail,
                    DistributedCacheSoftTimeout = cacheSettings.DistributedCacheSoftTimeout,
                    DistributedCacheHardTimeout = cacheSettings.DistributedCacheHardTimeout
                })
                .WithSystemTextJsonSerializer(jsonSerializerOptions)
                .TryWithAutoSetup();

            if (redisSettings.BackplaneEnabled && !string.IsNullOrWhiteSpace(redisConnectionString))
            {
                fusionBuilder.WithStackExchangeRedisBackplane(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }

            services.AddSingleton<IWebshopCache, WebshopCache>();
            services.AddSingleton<IWebshopCacheKeys, WebshopCacheKeys>();
            services.AddSingleton<IWebshopCacheInvalidationService, WebshopCacheInvalidationService>();

            services.AddSingleton<IOpenSearchClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<OpenSearchSettings>>().Value;
                var endpoint = ResolveOpenSearchUri(options, configuration);

                var connectionSettings = new ConnectionSettings(new Uri(endpoint))
                    .DefaultIndex(options.IndexName)
                    .DefaultMappingFor<ProductSearchDocument>(mapping => mapping
                        .IndexName(options.IndexName)
                        .IdProperty(document => document.ProductId))
                    .RequestTimeout(options.RequestTimeout);

                if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
                {
                    connectionSettings = connectionSettings.BasicAuthentication(options.Username, options.Password);
                }

                return new OpenSearchClient(connectionSettings);
            });

            services.AddScoped<IProductSearchService, OpenSearchProductSearchService>();
            services.AddScoped<IProductSearchIndexService, OpenSearchProductIndexService>();
            services.AddHostedService<ProductSearchReindexHostedService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureQueries(this IServiceCollection services)
        {
            services.TryAddSingleton<IWebshopCache, NoOpWebshopCache>();
            services.TryAddSingleton<IWebshopCacheKeys, NoOpWebshopCacheKeys>();
            services.TryAddSingleton<IWebshopCacheInvalidationService, NoOpWebshopCacheInvalidationService>();

            services.AddScoped<HomePageQueryService>();
            services.AddScoped<ProductListingQueryService>();
            services.AddScoped<ProductDetailQueryService>();
            services.AddScoped<BrandPageQueryService>();
            services.AddScoped<CollectionPageQueryService>();
            services.AddScoped<EditorialQueryService>();
            services.AddScoped<StoreQueryService>();

            services.AddScoped<IHomePageQueryService, CachedHomePageQueryService>();
            services.AddScoped<IProductListingQueryService, CachedProductListingQueryService>();
            services.AddScoped<IProductDetailQueryService, CachedProductDetailQueryService>();
            services.AddScoped<IBrandPageQueryService, CachedBrandPageQueryService>();
            services.AddScoped<ICollectionPageQueryService, CachedCollectionPageQueryService>();
            services.AddScoped<IEditorialQueryService, CachedEditorialQueryService>();
            services.AddScoped<IStoreQueryService, CachedStoreQueryService>();
            services.AddScoped<DevelopmentDataSeeder>();

            return services;
        }

        public static IServiceCollection AddCartServices(this IServiceCollection services)
        {
            services.AddScoped<ICartService, CartService>();

            return services;
        }

        public static IServiceCollection AddAdminServices(this IServiceCollection services)
        {
            services.AddScoped<IBrandAdminService, BrandAdminService>();
            services.AddScoped<ICollectionAdminService, CollectionAdminService>();
            services.AddScoped<IProductAdminService, ProductAdminService>();
            services.AddScoped<IProductVariantAdminService, ProductVariantAdminService>();
            services.AddScoped<IProductMediaAdminService, ProductMediaAdminService>();
            services.AddScoped<IStoreAdminService, StoreAdminService>();
            services.AddScoped<IEditorialAdminService, EditorialAdminService>();
            services.AddScoped<ITrustPageAdminService, TrustPageAdminService>();
            services.AddScoped<IHomePageAdminService, HomePageAdminService>();
            services.AddScoped<IBrandPageContentAdminService, BrandPageContentAdminService>();
            services.AddScoped<ICollectionPageContentAdminService, CollectionPageContentAdminService>();
            services.AddScoped<IStorePageContentAdminService, StorePageContentAdminService>();

            return services;
        }

        public static IServiceCollection AddWishlistServices(this IServiceCollection services)
        {
            services.AddScoped<IWishlistService, WishlistService>();

            return services;
        }

        private static string? ResolveRedisConnectionString(IConfiguration configuration, RedisSettings redisSettings)
        {
            if (!string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
            {
                return redisSettings.ConnectionString;
            }

            return configuration.GetConnectionString("redis")
                   ?? configuration.GetConnectionString("Redis");
        }

        private static string ResolveOpenSearchUri(OpenSearchSettings settings, IConfiguration configuration)
        {
            var configuredUri = configuration["OpenSearch:Uri"];
            if (!string.IsNullOrWhiteSpace(configuredUri))
            {
                return configuredUri;
            }

            return settings.Uri;
        }
    }
}
