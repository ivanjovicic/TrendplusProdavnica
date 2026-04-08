#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class BrandPageQueryService : IBrandPageQueryService
    {
        private readonly TrendplusDbContext _db;

        public BrandPageQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query)
        {
            var brand = await _db.Brands.AsNoTracking()
                .Where(entity => entity.Slug == query.Slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (brand is null)
            {
                throw new KeyNotFoundException($"Brand '{query.Slug}' was not found.");
            }

            var content = await _db.BrandPageContents.AsNoTracking()
                .Where(entity => entity.BrandId == brand.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroText,
                    entity.Faq,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            var productsQuery = ProductQueryMappingHelper
                .ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                .Where(product => product.BrandId == brand.Id);
            var featuredProjections = await ProductQueryMappingHelper
                .ToProductCardProjection(
                    productsQuery
                        .OrderByDescending(product => product.IsBestseller)
                        .ThenByDescending(product => product.SortRank)
                        .ThenByDescending(product => product.PublishedAtUtc)
                        .Take(12),
                    _db.Brands.AsNoTracking())
                .ToArrayAsync();

            var categoryIds = await productsQuery
                .Select(product => product.PrimaryCategoryId)
                .Concat(productsQuery.SelectMany(product => product.CategoryMaps.Select(map => map.CategoryId)))
                .Distinct()
                .ToArrayAsync();

            var categoryLinks = categoryIds.Length == 0
                ? Array.Empty<BreadcrumbItemDto>()
                : await _db.Categories.AsNoTracking()
                    .Where(category => categoryIds.Contains(category.Id) && category.IsActive)
                    .OrderBy(category => category.SortOrder)
                    .ThenBy(category => category.Name)
                    .Select(category => new BreadcrumbItemDto(
                        category.Name,
                        $"/kategorija/{category.Slug}"))
                    .ToArrayAsync();

            var introText = content?.IntroText ?? brand.LongDescription ?? brand.ShortDescription ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(
                content?.Seo ?? brand.Seo,
                brand.Name,
                brand.ShortDescription ?? string.Empty);

            return new BrandPageDto(
                brand.Name,
                brand.Slug,
                introText,
                seo,
                ProductQueryMappingHelper.ToProductCardDtos(featuredProjections),
                categoryLinks,
                MapFaq(content?.Faq));
        }

        private static FaqItemDto[]? MapFaq(IEnumerable<Domain.ValueObjects.FaqItem>? faq)
        {
            if (faq is null)
            {
                return null;
            }

            var mapped = faq
                .Select(item => new FaqItemDto(item.Question, item.Answer))
                .ToArray();

            return mapped.Length == 0 ? null : mapped;
        }
    }
}
