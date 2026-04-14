using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Analytics.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Analytics;
using TrendplusProdavnica.Infrastructure.Persistence;
using Xunit;

namespace TrendplusProdavnica.Tests
{
    public class ShoeTypeAnalyticsTests
    {
        private ServiceProvider CreateServiceProvider(string dbName)
        {
            var services = new ServiceCollection();
            services.AddLogging(x => x.AddConsole());
            services.AddDbContext<TrendplusDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            return services.BuildServiceProvider();
        }

        private async Task<(Category category, Product product, Order order)> SeedCategoryWithOrderAsync(
            TrendplusDbContext db,
            string categoryName = "Sneakers")
        {
            var now = DateTimeOffset.UtcNow;

            var category = new Category { Name = categoryName, Slug = categoryName.ToLower() };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var product = new Product
            {
                Name = "Shoe1",
                Slug = "shoe-1",
                BrandId = 1,
                PrimaryCategoryId = category.Id,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = "shoe-1-m",
                SizeEu = 42,
                Price = 150m,
                TotalStock = 10,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync();

            var order = new Order
            {
                OrderNumber = "ORD-SHOE-1",
                Status = OrderStatus.Completed,
                Currency = "RSD",
                DeliveryMethod = Domain.Sales.DeliveryMethod.Courier,
                PaymentMethod = Domain.Sales.PaymentMethod.CashOnDelivery,
                CustomerFirstName = "Jane",
                CustomerLastName = "Doe",
                Email = "jane@example.com",
                Phone = "+111111",
                DeliveryAddressLine1 = "Addr",
                DeliveryCity = "City",
                DeliveryPostalCode = "00000",
                SubtotalAmount = 150m,
                DeliveryAmount = 10m,
                TotalAmount = 160m,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                PlacedAtUtc = now
            };

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant.Id,
                ProductNameSnapshot = product.Name,
                BrandNameSnapshot = "BrandX",
                CategoryIdSnapshot = category.Id,
                CategoryNameSnapshot = category.Name,
                Quantity = 1,
                UnitPrice = 150m,
                LineTotal = 150m
            });

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return (category, product, order);
        }

        [Fact]
        public async Task GetShoeTypeSalesStatsAsync_EmptyDatabase_ReturnsEmptyReport()
        {
            const string dbName = "ShoeType_Empty";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            var report = await analyticsService.GetShoeTypeSalesStatsAsync();

            Assert.NotNull(report);
            Assert.Empty(report.ShoeTypes);
            Assert.Equal(0m, report.TotalMarketRevenue);
            Assert.Equal(0, report.TotalMarketOrders);
        }

        [Fact]
        public async Task GetShoeTypeSalesStatsAsync_SingleCategory_ReturnsCorrectTotals()
        {
            const string dbName = "ShoeType_Single";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            var (category, product, order) = await SeedCategoryWithOrderAsync(db);

            var report = await analyticsService.GetShoeTypeSalesStatsAsync();

            Assert.NotEmpty(report.ShoeTypes);
            var stat = report.ShoeTypes.First();
            Assert.Equal(category.Id, stat.CategoryId);
            Assert.Equal(1, stat.TotalOrders);
            Assert.Equal(150m, stat.TotalRevenue);
        }

        [Fact]
        public async Task GetShoeTypeSalesStatsAsync_Concurrency_IsThreadSafe()
        {
            const string dbName = "ShoeType_Concurrency";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            await SeedCategoryWithOrderAsync(db);

            var tasks = Enumerable.Range(0, 8).Select(_ => analyticsService.GetShoeTypeSalesStatsAsync());
            var results = await Task.WhenAll(tasks);

            var firstRevenue = results[0].TotalMarketRevenue;
            foreach (var r in results)
            {
                Assert.Equal(firstRevenue, r.TotalMarketRevenue);
            }
        }
    }
}
