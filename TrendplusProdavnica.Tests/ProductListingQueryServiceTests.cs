using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.DependencyInjection;
using TrendplusProdavnica.Infrastructure.Persistence;
using Xunit;

namespace TrendplusProdavnica.Tests
{
    public class ProductListingQueryServiceTests
    {
        [Fact]
        public async Task GetCategoryListingAsync_Returns_Published_ProductCard()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TrendplusDbContext>(options => options.UseInMemoryDatabase("ProductListingCategoryTest"));
            services.AddInfrastructureQueries();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();

            var now = DateTimeOffset.UtcNow;
            var category = new Category
            {
                Name = "Salonke",
                Slug = "salonke",
                Depth = 1,
                SortOrder = 1,
                IsActive = true,
                Type = CategoryType.Subcategory,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.Categories.Add(category);

            var brand = new Brand
            {
                Name = "TrendPlus",
                Slug = "trendplus",
                IsActive = true,
                SortOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            var product = new Product
            {
                BrandId = brand.Id,
                PrimaryCategoryId = category.Id,
                Name = "Salonke Nova",
                Slug = "salonke-nova",
                ShortDescription = "Lagane salonke za prolece",
                IsVisible = true,
                IsPurchasable = true,
                IsNew = true,
                IsBestseller = false,
                Status = ProductStatus.Published,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = "TP-1001",
                SizeEu = 37,
                Price = 5990m,
                Currency = "RSD",
                StockStatus = StockStatus.InStock,
                TotalStock = 12,
                IsActive = true,
                IsVisible = true,
                SortOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.ProductVariants.Add(variant);

            var media = new ProductMedia
            {
                ProductId = product.Id,
                Url = "https://cdn.example.com/salonke1.jpg",
                MediaType = MediaType.Image,
                MediaRole = MediaRole.Gallery,
                SortOrder = 1,
                IsPrimary = true,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.ProductMedia.Add(media);
            await db.SaveChangesAsync();

            var listingService = scope.ServiceProvider.GetRequiredService<IProductListingQueryService>();
            var result = await listingService.GetCategoryListingAsync(new GetCategoryListingQuery("salonke", 1, 10));

            Assert.NotNull(result);
            Assert.Single(result.Products);
            Assert.Equal("salonke-nova", result.Products[0].Slug);
            Assert.Equal("TrendPlus", result.Products[0].BrandName);
            Assert.Equal(5990m, result.Products[0].Price);
            Assert.True(result.Products[0].IsInStock);
            Assert.Equal(1, result.Products[0].AvailableSizesCount);
        }
    }
}
