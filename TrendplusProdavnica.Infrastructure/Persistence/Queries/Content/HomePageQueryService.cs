#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Domain.ValueObjects;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class HomePageQueryService : IHomePageQueryService
    {
        private readonly TrendplusDbContext _db;

        public HomePageQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<HomePageDto> GetHomePageAsync()
        {
            var page = await _db.HomePages.AsNoTracking()
                .Where(entity => entity.IsPublished)
                .OrderByDescending(entity => entity.PublishedAtUtc)
                .ThenByDescending(entity => entity.Id)
                .Select(entity => new
                {
                    entity.Title,
                    entity.Seo,
                    entity.Modules
                })
                .FirstOrDefaultAsync();

            if (page is null)
            {
                return EmptyHomePage();
            }

            var modules = page.Modules?.ToArray() ?? Array.Empty<HomeModule>();
            var seo = ProductQueryMappingHelper.MapSeo(page.Seo, page.Title, string.Empty);

            var heroModule = FindModule(modules, "heroSection", "hero");
            var announcementModule = FindModule(modules, "announcementBar", "announcement");
            var categoryCardsModule = FindModule(modules, "categoryCards", "categories");
            var newArrivalsModule = FindModule(modules, "newArrivals");
            var featuredCollectionsModule = FindModule(modules, "featuredCollections", "collections");
            var bestsellersModule = FindModule(modules, "bestsellers");
            var brandWallModule = FindModule(modules, "brandWall", "brands");
            var editorialModule = FindModule(modules, "editorialStatement", "editorial");
            var storeTeaserModule = FindModule(modules, "storeTeaser", "stores");
            var trustItemsModule = FindModule(modules, "trustItems", "trust");
            var newsletterModule = FindModule(modules, "newsletter");

            var categoryCards = await BuildCategoryCardsAsync(categoryCardsModule);
            var newArrivals = await BuildNewArrivalsAsync(newArrivalsModule);
            var featuredCollections = await BuildFeaturedCollectionsAsync(featuredCollectionsModule);
            var bestsellers = await BuildBestsellersAsync(bestsellersModule);
            var brandWall = await BuildBrandWallAsync(brandWallModule);
            var storeTeaser = await BuildStoreTeaserAsync(storeTeaserModule);

            return new HomePageDto(
                seo,
                BuildAnnouncementBar(announcementModule),
                BuildHeroSection(heroModule, page.Title),
                categoryCards,
                newArrivals,
                featuredCollections,
                bestsellers,
                brandWall,
                BuildEditorialStatement(editorialModule),
                storeTeaser,
                BuildTrustItems(trustItemsModule),
                BuildNewsletter(newsletterModule));
        }

        private async Task<CategoryCardDto[]> BuildCategoryCardsAsync(HomeModule? module)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return Array.Empty<CategoryCardDto>();
            }

            var categoryIds = ReadLongArray(payload, "categoryIds", "ids");

            if (categoryIds.Length > 0)
            {
                var categories = await _db.Categories.AsNoTracking()
                    .Where(category => categoryIds.Contains(category.Id) && category.IsActive)
                    .Select(category => new
                    {
                        category.Id,
                        Card = new CategoryCardDto(
                            category.Name,
                            category.Slug,
                            category.ImageUrl)
                    })
                    .ToArrayAsync();

                var order = categoryIds
                    .Select((id, index) => new { id, index })
                    .ToDictionary(item => item.id, item => item.index);

                return categories
                    .OrderBy(item => order.TryGetValue(item.Id, out var index) ? index : int.MaxValue)
                    .Select(item => item.Card)
                    .ToArray();
            }

            var cards = ReadArray(payload, "items", "cards")
                .Select(item => new CategoryCardDto(
                    ReadString(item, "name", "title") ?? string.Empty,
                    ReadString(item, "slug") ?? string.Empty,
                    ReadString(item, "imageUrl", "image")))
                .Where(card => !string.IsNullOrWhiteSpace(card.Name) && !string.IsNullOrWhiteSpace(card.Slug))
                .ToArray();

            return cards;
        }

        private async Task<ProductCardDto[]> BuildNewArrivalsAsync(HomeModule? module)
        {
            if (module is null)
            {
                return Array.Empty<ProductCardDto>();
            }

            var limit = ReadLimit(module, 12);
            var products = ProductQueryMappingHelper
                .ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                .Where(product => product.IsNew)
                .OrderByDescending(product => product.PublishedAtUtc)
                .ThenByDescending(product => product.SortRank);

            return await LoadProductCardsAsync(products, limit);
        }

        private async Task<ProductCardDto[]> BuildBestsellersAsync(HomeModule? module)
        {
            if (module is null)
            {
                return Array.Empty<ProductCardDto>();
            }

            var limit = ReadLimit(module, 12);
            var products = ProductQueryMappingHelper
                .ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                .Where(product => product.IsBestseller)
                .OrderByDescending(product => product.SortRank)
                .ThenByDescending(product => product.PublishedAtUtc);

            return await LoadProductCardsAsync(products, limit);
        }

        private async Task<CollectionTeaserDto[]> BuildFeaturedCollectionsAsync(HomeModule? module)
        {
            if (module is null)
            {
                return Array.Empty<CollectionTeaserDto>();
            }

            var limit = ReadLimit(module, 6);
            var collectionIds = TryGetPayload(module, out var payload)
                ? ReadLongArray(payload, "collectionIds", "ids")
                : Array.Empty<long>();

            var collectionsQuery = _db.Collections.AsNoTracking()
                .Where(collection => collection.IsActive);

            if (collectionIds.Length > 0)
            {
                collectionsQuery = collectionsQuery.Where(collection => collectionIds.Contains(collection.Id));
            }
            else
            {
                collectionsQuery = collectionsQuery.Where(collection => collection.IsFeatured);
            }

            return await collectionsQuery
                .OrderBy(collection => collection.SortOrder)
                .ThenBy(collection => collection.Name)
                .Take(limit)
                .Select(collection => new CollectionTeaserDto(
                    collection.Name,
                    collection.Slug,
                    collection.CoverImageUrl,
                    collection.ShortDescription))
                .ToArrayAsync();
        }

        private async Task<BrandWallItemDto[]> BuildBrandWallAsync(HomeModule? module)
        {
            if (module is null)
            {
                return Array.Empty<BrandWallItemDto>();
            }

            var limit = ReadLimit(module, 12);

            return await _db.Brands.AsNoTracking()
                .Where(brand => brand.IsActive && brand.IsFeatured)
                .OrderBy(brand => brand.SortOrder)
                .ThenBy(brand => brand.Name)
                .Take(limit)
                .Select(brand => new BrandWallItemDto(
                    brand.Name,
                    brand.Slug,
                    brand.LogoUrl))
                .ToArrayAsync();
        }

        private async Task<StoreTeaserDto?> BuildStoreTeaserAsync(HomeModule? module)
        {
            if (module is null)
            {
                return null;
            }

            var storesQuery = _db.Stores.AsNoTracking()
                .Where(store => store.IsActive);

            if (TryGetPayload(module, out var payload))
            {
                var storeSlug = ReadString(payload, "storeSlug", "slug");
                var storeId = ReadLong(payload, "storeId", "id");

                if (!string.IsNullOrWhiteSpace(storeSlug))
                {
                    storesQuery = storesQuery.Where(store => store.Slug == storeSlug);
                }
                else if (storeId.HasValue)
                {
                    storesQuery = storesQuery.Where(store => store.Id == storeId.Value);
                }
            }

            return await storesQuery
                .OrderBy(store => store.SortOrder)
                .ThenBy(store => store.Name)
                .Take(5)
                .Select(store => new StoreTeaserDto(
                    store.Name,
                    store.Slug,
                    store.CoverImageUrl ?? string.Empty))
                .FirstOrDefaultAsync();
        }

        private static AnnouncementBarDto? BuildAnnouncementBar(HomeModule? module)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return null;
            }

            var text = ReadString(payload, "text", "title");

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return new AnnouncementBarDto(
                text,
                ReadString(payload, "backgroundColor"),
                ReadString(payload, "textColor"),
                ReadString(payload, "callToActionUrl", "ctaUrl", "url"));
        }

        private static HeroSectionDto BuildHeroSection(HomeModule? module, string fallbackTitle)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return new HeroSectionDto(fallbackTitle, string.Empty, string.Empty);
            }

            return new HeroSectionDto(
                ReadString(payload, "title") ?? fallbackTitle,
                ReadString(payload, "subtitle") ?? string.Empty,
                ReadString(payload, "imageUrl", "image") ?? string.Empty);
        }

        private static EditorialStatementDto? BuildEditorialStatement(HomeModule? module)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return null;
            }

            var title = ReadString(payload, "title");
            var text = ReadString(payload, "text", "body");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return new EditorialStatementDto(title, text);
        }

        private static TrustItemDto[] BuildTrustItems(HomeModule? module)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return Array.Empty<TrustItemDto>();
            }

            return ReadArray(payload, "items")
                .Select(item => new TrustItemDto(
                    ReadString(item, "title") ?? string.Empty,
                    ReadString(item, "description", "text") ?? string.Empty))
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.Title) &&
                    !string.IsNullOrWhiteSpace(item.Description))
                .ToArray();
        }

        private static NewsletterDto? BuildNewsletter(HomeModule? module)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return null;
            }

            var title = ReadString(payload, "title");
            var placeholder = ReadString(payload, "placeholder");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(placeholder))
            {
                return null;
            }

            return new NewsletterDto(title, placeholder);
        }

        private async Task<ProductCardDto[]> LoadProductCardsAsync(
            IQueryable<Domain.Catalog.Product> products,
            int take)
        {
            var projections = await ProductQueryMappingHelper
                .ToProductCardProjection(products.Take(take), _db.Brands.AsNoTracking())
                .ToArrayAsync();

            return ProductQueryMappingHelper.ToProductCardDtos(projections);
        }

        private static HomeModule? FindModule(IEnumerable<HomeModule> modules, params string[] aliases)
        {
            var aliasSet = new HashSet<string>(aliases.Select(alias => alias.ToLowerInvariant()));

            return modules.FirstOrDefault(module =>
                !string.IsNullOrWhiteSpace(module.Type) &&
                aliasSet.Contains(module.Type.Trim().ToLowerInvariant()));
        }

        private static bool TryGetPayload(HomeModule? module, out JsonElement payload)
        {
            payload = default;

            if (module?.Payload is not JsonElement element)
            {
                return false;
            }

            if (element.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            {
                payload = element;
                return true;
            }

            return false;
        }

        private static int ReadLimit(HomeModule module, int fallback)
        {
            if (!TryGetPayload(module, out var payload))
            {
                return fallback;
            }

            if (payload.ValueKind == JsonValueKind.Object &&
                payload.TryGetProperty("limit", out var limitElement) &&
                limitElement.ValueKind == JsonValueKind.Number &&
                limitElement.TryGetInt32(out var parsedLimit))
            {
                return Math.Clamp(parsedLimit, 1, 24);
            }

            return fallback;
        }

        private static string? ReadString(JsonElement element, params string[] keys)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var key in keys)
            {
                if (element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }

            return null;
        }

        private static long? ReadLong(JsonElement element, params string[] keys)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var key in keys)
            {
                if (element.TryGetProperty(key, out var value) &&
                    value.ValueKind == JsonValueKind.Number &&
                    value.TryGetInt64(out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static long[] ReadLongArray(JsonElement element, params string[] keys)
        {
            var items = ReadArray(element, keys)
                .Where(item => item.ValueKind == JsonValueKind.Number && item.TryGetInt64(out _))
                .Select(item => item.GetInt64())
                .Distinct()
                .ToArray();

            return items;
        }

        private static IEnumerable<JsonElement> ReadArray(JsonElement element, params string[] keys)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray().ToArray();
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<JsonElement>();
            }

            foreach (var key in keys)
            {
                if (element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Array)
                {
                    return value.EnumerateArray().ToArray();
                }
            }

            return Array.Empty<JsonElement>();
        }

        private static HomePageDto EmptyHomePage()
        {
            return new HomePageDto(
                new SeoDto(string.Empty, string.Empty, null, null),
                null,
                new HeroSectionDto(string.Empty, string.Empty, string.Empty),
                Array.Empty<CategoryCardDto>(),
                Array.Empty<ProductCardDto>(),
                Array.Empty<CollectionTeaserDto>(),
                Array.Empty<ProductCardDto>(),
                Array.Empty<BrandWallItemDto>(),
                null,
                null,
                Array.Empty<TrustItemDto>(),
                null);
        }
    }
}
