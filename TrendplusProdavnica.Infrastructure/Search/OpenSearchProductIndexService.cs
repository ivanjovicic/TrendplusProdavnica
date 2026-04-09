#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class OpenSearchProductIndexService : IProductSearchIndexService
    {
        private readonly TrendplusDbContext _db;
        private readonly IOpenSearchClient _client;
        private readonly OpenSearchSettings _settings;
        private readonly ILogger<OpenSearchProductIndexService> _logger;

        public OpenSearchProductIndexService(
            TrendplusDbContext db,
            IOpenSearchClient client,
            IOptions<OpenSearchSettings> settings,
            ILogger<OpenSearchProductIndexService> logger)
        {
            _db = db;
            _client = client;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
        {
            await EnsureIndexAsync(cancellationToken);

            var eligibleIds = await _db.Products.AsNoTracking()
                .Where(product =>
                    product.Status == ProductStatus.Published &&
                    product.IsVisible &&
                    product.IsPurchasable &&
                    !product.SearchHidden)
                .Select(product => product.Id)
                .ToArrayAsync(cancellationToken);

            var documents = await BuildDocumentsAsync(eligibleIds, cancellationToken);

            var clearResponse = await _client.DeleteByQueryAsync<ProductSearchDocument>(descriptor => descriptor
                    .Index(_settings.IndexName)
                    .Query(query => query.MatchAll()),
                cancellationToken);

            if (!clearResponse.IsValid)
            {
                throw new InvalidOperationException(
                    $"OpenSearch delete-by-query failed before reindex: {clearResponse.ServerError?.ToString() ?? clearResponse.OriginalException?.Message}");
            }

            if (documents.Length > 0)
            {
                var bulkResponse = await _client.IndexManyAsync(documents, _settings.IndexName, cancellationToken);

                if (!bulkResponse.IsValid || bulkResponse.Errors)
                {
                    var firstError = bulkResponse.ItemsWithErrors?.FirstOrDefault()?.Error?.Reason;
                    throw new InvalidOperationException(
                        $"OpenSearch bulk index failed: {firstError ?? bulkResponse.ServerError?.ToString() ?? bulkResponse.OriginalException?.Message}");
                }
            }

            await _client.Indices.RefreshAsync(_settings.IndexName, descriptor => descriptor, cancellationToken);
            _logger.LogInformation("OpenSearch product reindex completed. Indexed {Count} documents.", documents.Length);
        }

        public async Task ReindexProductAsync(long productId, CancellationToken cancellationToken = default)
        {
            await EnsureIndexAsync(cancellationToken);

            var documents = await BuildDocumentsAsync(new[] { productId }, cancellationToken);

            if (documents.Length == 0)
            {
                await DeleteProductAsync(productId, cancellationToken);
                return;
            }

            var indexResponse = await _client.IndexDocumentAsync(documents[0], cancellationToken);
            if (!indexResponse.IsValid)
            {
                throw new InvalidOperationException(
                    $"OpenSearch index document failed for product {productId}: {indexResponse.ServerError?.ToString() ?? indexResponse.OriginalException?.Message}");
            }

            await _client.Indices.RefreshAsync(_settings.IndexName, descriptor => descriptor, cancellationToken);
        }

        public async Task DeleteProductAsync(long productId, CancellationToken cancellationToken = default)
        {
            var existsResponse = await _client.Indices.ExistsAsync(_settings.IndexName, descriptor => descriptor, cancellationToken);
            if (!existsResponse.Exists)
            {
                return;
            }

            var deleteResponse = await _client.DeleteByQueryAsync<ProductSearchDocument>(descriptor => descriptor
                    .Index(_settings.IndexName)
                    .Query(query => query.Term(term => term
                        .Field(field => field.ProductId)
                        .Value(productId))),
                cancellationToken);

            if (!deleteResponse.IsValid)
            {
                throw new InvalidOperationException(
                    $"OpenSearch delete-by-query failed for product {productId}: {deleteResponse.ServerError?.ToString() ?? deleteResponse.OriginalException?.Message}");
            }
        }

        private async Task EnsureIndexAsync(CancellationToken cancellationToken)
        {
            var existsResponse = await _client.Indices.ExistsAsync(_settings.IndexName, descriptor => descriptor, cancellationToken);
            if (existsResponse.Exists)
            {
                return;
            }

            if (!_settings.AutoCreateIndex)
            {
                throw new InvalidOperationException($"OpenSearch index '{_settings.IndexName}' does not exist and auto-create is disabled.");
            }

            var createResponse = await _client.Indices.CreateAsync(_settings.IndexName, descriptor => descriptor
                .Settings(settings => settings
                    .NumberOfShards(1)
                    .NumberOfReplicas(0))
                .Map<ProductSearchDocument>(map => map
                    .AutoMap()
                    .Properties(properties => properties
                        .Keyword(keyword => keyword.Name(field => field.Slug))
                        .Text(text => text.Name(field => field.BrandName).Fields(fields => fields.Keyword(keyword => keyword.Name("keyword"))))
                        .Text(text => text.Name(field => field.Name))
                        .Text(text => text.Name(field => field.ShortDescription))
                        .Text(text => text.Name(field => field.PrimaryCategory).Fields(fields => fields.Keyword(keyword => keyword.Name("keyword"))))
                        .Text(text => text.Name(field => field.SecondaryCategories).Fields(fields => fields.Keyword(keyword => keyword.Name("keyword"))))
                        .Text(text => text.Name(field => field.PrimaryColorName).Fields(fields => fields.Keyword(keyword => keyword.Name("keyword"))))
                        .Text(text => text.Name(field => field.SearchKeywords))
                        .Number(number => number.Name(field => field.MinPrice).Type(NumberType.Double))
                        .Number(number => number.Name(field => field.MaxPrice).Type(NumberType.Double))
                        .Number(number => number.Name(field => field.AvailableSizes).Type(NumberType.Double)))),
                cancellationToken);

            if (!createResponse.IsValid)
            {
                throw new InvalidOperationException(
                    $"OpenSearch index creation failed for '{_settings.IndexName}': {createResponse.ServerError?.ToString() ?? createResponse.OriginalException?.Message}");
            }

            _logger.LogInformation("OpenSearch index '{IndexName}' created.", _settings.IndexName);
        }

        private async Task<ProductSearchDocument[]> BuildDocumentsAsync(long[] productIds, CancellationToken cancellationToken)
        {
            if (productIds.Length == 0)
            {
                return Array.Empty<ProductSearchDocument>();
            }

            var headers = await (
                    from product in _db.Products.AsNoTracking()
                    join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                    join primaryCategory in _db.Categories.AsNoTracking() on product.PrimaryCategoryId equals primaryCategory.Id into categoryJoin
                    from primaryCategory in categoryJoin.DefaultIfEmpty()
                    where productIds.Contains(product.Id) &&
                          product.Status == ProductStatus.Published &&
                          product.IsVisible &&
                          product.IsPurchasable &&
                          !product.SearchHidden
                    select new
                    {
                        product.Id,
                        product.Slug,
                        product.Name,
                        product.ShortDescription,
                        product.PrimaryColorName,
                        product.IsNew,
                        product.IsBestseller,
                        product.SearchKeywords,
                        product.SearchSynonyms,
                        product.SortRank,
                        product.PublishedAtUtc,
                        BrandName = brand.Name,
                        PrimaryCategory = primaryCategory != null ? primaryCategory.Name : null
                    })
                .ToArrayAsync(cancellationToken);

            if (headers.Length == 0)
            {
                return Array.Empty<ProductSearchDocument>();
            }

            var headerIds = headers.Select(header => header.Id).ToArray();

            var variants = await _db.ProductVariants.AsNoTracking()
                .Where(variant =>
                    headerIds.Contains(variant.ProductId) &&
                    variant.IsActive &&
                    variant.IsVisible)
                .Select(variant => new
                {
                    variant.ProductId,
                    variant.Price,
                    variant.OldPrice,
                    variant.SizeEu,
                    variant.TotalStock
                })
                .ToArrayAsync(cancellationToken);

            var mediaRows = await _db.ProductMedia.AsNoTracking()
                .Where(media =>
                    headerIds.Contains(media.ProductId) &&
                    media.IsActive)
                .Select(media => new
                {
                    media.ProductId,
                    media.Url,
                    media.IsPrimary,
                    media.SortOrder,
                    media.Id
                })
                .ToArrayAsync(cancellationToken);

            var secondaryCategoryRows = await (
                    from map in _db.ProductCategoryMaps.AsNoTracking()
                    join category in _db.Categories.AsNoTracking() on map.CategoryId equals category.Id
                    where headerIds.Contains(map.ProductId)
                    orderby map.SortOrder, map.CategoryId
                    select new
                    {
                        map.ProductId,
                        CategoryName = category.Name
                    })
                .ToArrayAsync(cancellationToken);

            var variantLookup = variants
                .GroupBy(variant => variant.ProductId)
                .ToDictionary(group => group.Key, group => group.ToArray());

            var primaryMediaLookup = mediaRows
                .GroupBy(media => media.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(media => media.IsPrimary)
                        .ThenBy(media => media.SortOrder)
                        .ThenBy(media => media.Id)
                        .Select(media => media.Url)
                        .FirstOrDefault());

            var secondaryCategoryLookup = secondaryCategoryRows
                .GroupBy(row => row.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(row => row.CategoryName)
                        .Distinct()
                        .ToArray());

            var documents = new List<ProductSearchDocument>(headers.Length);

            foreach (var header in headers)
            {
                if (!variantLookup.TryGetValue(header.Id, out var productVariants) || productVariants.Length == 0)
                {
                    continue;
                }

                var minPrice = productVariants.Min(variant => (double)variant.Price);
                var maxPrice = productVariants.Max(variant => (double)variant.Price);
                var isOnSale = productVariants.Any(variant => variant.OldPrice.HasValue && variant.OldPrice.Value > variant.Price);
                var inStock = productVariants.Any(variant => variant.TotalStock > 0);
                var sizes = productVariants
                    .Select(variant => (double)variant.SizeEu)
                    .Distinct()
                    .OrderBy(size => size)
                    .ToArray();

                var source = new ProductSearchSource(
                    header.Id,
                    header.Slug,
                    header.BrandName,
                    header.Name,
                    header.ShortDescription,
                    header.PrimaryCategory,
                    secondaryCategoryLookup.TryGetValue(header.Id, out var secondaryCategories)
                        ? secondaryCategories
                        : Array.Empty<string>(),
                    header.PrimaryColorName,
                    header.IsNew,
                    header.IsBestseller,
                    isOnSale,
                    minPrice,
                    maxPrice,
                    sizes,
                    inStock,
                    primaryMediaLookup.TryGetValue(header.Id, out var primaryImageUrl)
                        ? primaryImageUrl
                        : null,
                    header.SearchKeywords,
                    header.SearchSynonyms,
                    header.SortRank,
                    header.PublishedAtUtc);

                documents.Add(ProductSearchDocumentMapper.Map(source));
            }

            return documents.ToArray();
        }
    }
}
