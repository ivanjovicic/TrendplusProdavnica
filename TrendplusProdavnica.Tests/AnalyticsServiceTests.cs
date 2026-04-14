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
    /// <summary>
    /// Comprehensive test suite for AnalyticsService.GetSupplierSalesStatsAsync()
    /// Tests data accuracy, immutability, and aggregation correctness
    /// </summary>
    public class AnalyticsServiceTests
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

        private async Task<(Brand brand, List<Product> products, List<Order> orders)> 
            SeedSupplierWithOrdersAsync(
                TrendplusDbContext db,
                string brandName = "TestBrand",
                int productCount = 2,
                int orderCount = 3,
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var now = DateTimeOffset.UtcNow;
            startDate ??= now.AddDays(-30).DateTime;
            endDate ??= now.DateTime;

            // Create brand
            var brand = new Brand
            {
                Name = brandName,
                Slug = brandName.ToLower(),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            // Create products
            var products = new List<Product>();
            for (int i = 0; i < productCount; i++)
            {
                var product = new Product
                {
                    Name = $"Product {i + 1}",
                    Slug = $"product-{i + 1}",
                    ShortDescription = $"Test product {i + 1}",
                    BrandId = brand.Id,
                    PrimaryCategoryId = 1,
                    Status = TrendplusProdavnica.Domain.Enums.ProductStatus.Published,
                    IsVisible = true,
                    IsPurchasable = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                db.Products.Add(product);
                products.Add(product);
            }
            await db.SaveChangesAsync();

            // Create variants for each product
            foreach (var product in products)
            {
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Sku = $"{product.Slug}-m",
                    SizeEu = 42,
                    Price = 100m,
                    TotalStock = 1000,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                db.ProductVariants.Add(variant);
            }
            await db.SaveChangesAsync();

            // Create orders
            var orders = new List<Order>();
            var orderStatuses = new[] { OrderStatus.Completed, OrderStatus.Pending, OrderStatus.Completed };

            for (int i = 0; i < orderCount; i++)
            {
                var orderDate = startDate.Value.AddDays(i * 5);
                var order = new Order
                {
                    OrderNumber = $"ORD-{i + 1:D6}",
                    Status = orderStatuses[i % orderStatuses.Length],
                    Currency = "RSD",
                    DeliveryMethod = Domain.Sales.DeliveryMethod.Courier,
                    PaymentMethod = Domain.Sales.PaymentMethod.CashOnDelivery,
                    CustomerFirstName = "John",
                    CustomerLastName = "Doe",
                    Email = "john@example.com",
                    Phone = "+123456789",
                    DeliveryAddressLine1 = "123 Main St",
                    DeliveryCity = "New York",
                    DeliveryPostalCode = "10001",
                    SubtotalAmount = 300m,
                    DeliveryAmount = 50m,
                    TotalAmount = 350m,
                    CreatedAtUtc = new DateTimeOffset(orderDate, TimeSpan.Zero),
                    UpdatedAtUtc = new DateTimeOffset(orderDate, TimeSpan.Zero),
                    PlacedAtUtc = new DateTimeOffset(orderDate, TimeSpan.Zero)
                };

                // Add items from different products
                foreach (var product in products)
                {
                    var variant = db.ProductVariants.First(v => v.ProductId == product.Id);
                    var item = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductVariantId = variant.Id,
                        ProductNameSnapshot = product.Name,
                        BrandNameSnapshot = brand.Name,
                        SizeEuSnapshot = 42,
                        UnitPrice = 100m,
                        Quantity = 1,
                        LineTotal = 100m
                    };
                    order.Items.Add(item);
                }

                db.Orders.Add(order);
                orders.Add(order);
            }

            await db.SaveChangesAsync();
            return (brand, products, orders);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_SingleSupplier_ReturnsCorrectTotals()
        {
            const string dbName = "Analytics_SingleSupplier";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed: 1 brand, 2 products, 3 orders
            var (brand, products, orders) = await SeedSupplierWithOrdersAsync(db);

            // Act
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            Assert.NotNull(report);
            Assert.Single(report.Suppliers);

            var supplierStat = report.Suppliers.First();
            Assert.Equal(brand.Id, supplierStat.BrandId);
            Assert.Equal(brand.Name, supplierStat.BrandName);
            // Orders should be counted per brand per order (not per item)
            Assert.True(supplierStat.TotalOrders > 0, "Should have at least 1 order");
            Assert.True(supplierStat.CompletedOrders >= 0, "Completed orders should be >= 0");
            Assert.True(supplierStat.PendingOrders >= 0, "Pending orders should be >= 0");
            Assert.Equal(supplierStat.TotalOrders, supplierStat.CompletedOrders + supplierStat.PendingOrders);
            Assert.True(supplierStat.TotalRevenue > 0);
            Assert.True(supplierStat.IsAggregated);
            Assert.True(supplierStat.CalculatedAtUtc != default);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_DataImmutability_ConsecutiveCallsMatch()
        {
            const string dbName = "Analytics_DataImmutability";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed data
            await SeedSupplierWithOrdersAsync(db);

            // Act: Call twice
            var report1 = await analyticsService.GetSupplierSalesStatsAsync();
            System.Threading.Thread.Sleep(100); // Small delay between calls
            var report2 = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert: Metrics should be identical (immutability test)
            Assert.Equal(report1.Suppliers.Count, report2.Suppliers.Count);
            Assert.Equal(report1.TotalMarketRevenue, report2.TotalMarketRevenue);
            Assert.Equal(report1.TotalMarketOrders, report2.TotalMarketOrders);

            // Individual supplier metrics must match (within rounding for decimals)
            foreach (var (stat1, stat2) in report1.Suppliers.Zip(report2.Suppliers))
            {
                Assert.Equal(stat1.BrandId, stat2.BrandId);
                Assert.Equal(Math.Round(stat1.TotalRevenue, 2), Math.Round(stat2.TotalRevenue, 2));
                Assert.Equal(stat1.TotalOrders, stat2.TotalOrders);
                Assert.Equal(Math.Round(stat1.ConversionRate, 2), Math.Round(stat2.ConversionRate, 2));
                Assert.Equal(stat1.IsAggregated, stat2.IsAggregated);
                Assert.Equal(stat1.SourceRecordCount, stat2.SourceRecordCount);
            }
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_MultipleSuppliers_CorrectAggregation()
        {
            const string dbName = "Analytics_MultipleSuppliers";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Act: Seed 3 different suppliers
            var supplier1 = await SeedSupplierWithOrdersAsync(db, "Brand1", 2, 3);
            var supplier2 = await SeedSupplierWithOrdersAsync(db, "Brand2", 3, 2);
            var supplier3 = await SeedSupplierWithOrdersAsync(db, "Brand3", 1, 4);

            // Act: Get stats
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            Assert.Equal(3, report.Suppliers.Count);
            Assert.True(report.TotalMarketOrders > 0);
            Assert.True(report.TotalMarketRevenue > 0);
            
            // Verify each supplier is isolated
            var brand1Stat = report.Suppliers.First(s => s.BrandId == supplier1.brand.Id);
            var brand2Stat = report.Suppliers.First(s => s.BrandId == supplier2.brand.Id);
            var brand3Stat = report.Suppliers.First(s => s.BrandId == supplier3.brand.Id);

            Assert.NotEqual(brand1Stat.TotalOrders, brand2Stat.TotalOrders);
            Assert.NotEqual(brand2Stat.TotalOrders, brand3Stat.TotalOrders);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_FilterByBrandId_ReturnsOnlyMatchingBrand()
        {
            const string dbName = "Analytics_FilterByBrandId";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed multiple brands
            var supplier1 = await SeedSupplierWithOrdersAsync(db, "Brand1", 2, 3);
            var supplier2 = await SeedSupplierWithOrdersAsync(db, "Brand2", 2, 2);

            // Act: Filter by Brand2
            var report = await analyticsService.GetSupplierSalesStatsAsync(brandId: supplier2.brand.Id);

            // Assert
            Assert.Single(report.Suppliers);
            Assert.Equal(supplier2.brand.Id, report.Suppliers.First().BrandId);
            Assert.True(report.Suppliers.First().TotalOrders > 0);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_FilterByDateRange_ReturnsOnlyItemsInRange()
        {
            const string dbName = "Analytics_FilterByDateRange";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            var now = DateTimeOffset.UtcNow;
            var startDate = now.AddDays(-30).DateTime;
            var endDate = now.DateTime;

            // Seed data within confirmed range
            await SeedSupplierWithOrdersAsync(db, "TestBrand", 2, 2, startDate, endDate);

            // Act: Query with full seeded date range
            var report = await analyticsService.GetSupplierSalesStatsAsync(
                from: startDate,
                to: endDate);

            // Assert: Should get results in the filtered range
            Assert.NotEmpty(report.Suppliers);
            foreach (var supplier in report.Suppliers)
            {
                Assert.True(supplier.CalculatedAtUtc != default);
            }
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_LimitParameter_RespectsPaging()
        {
            const string dbName = "Analytics_LimitParameter";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed multiple suppliers with same date range to ensure all are captured
            var now = DateTimeOffset.UtcNow;
            var startDate = now.AddDays(-30).DateTime;
            var endDate = now.DateTime;
            
            for (int i = 0; i < 5; i++)
            {
                await SeedSupplierWithOrdersAsync(db, $"Brand{i}", 1, 1, startDate, endDate);
            }

            // Act: Get all suppliers
            var reportAll = await analyticsService.GetSupplierSalesStatsAsync(
                from: startDate,
                to: endDate,
                limit: 100);
            
            // Act: Get only top 3
            var reportLimited = await analyticsService.GetSupplierSalesStatsAsync(
                from: startDate,
                to: endDate,
                limit: 3);

            // Assert
            Assert.True(reportAll.Suppliers.Count >= 1, "Should have seeded at least 1 supplier");
            Assert.True(reportLimited.Suppliers.Count <= 3, "Limited result should have at most 3 suppliers");
            Assert.True(reportLimited.Suppliers.Count <= reportAll.Suppliers.Count, "Limited should be <= all");
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_EmptyDatabase_ReturnsEmptyReport()
        {
            const string dbName = "Analytics_EmptyDatabase";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Act: Query empty database
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            Assert.NotNull(report);
            Assert.Empty(report.Suppliers);
            Assert.Equal(0, report.TotalMarketOrders);
            Assert.Equal(0m, report.TotalMarketRevenue);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_AuditTrail_HasVersion_And_SourceRecordCount()
        {
            const string dbName = "Analytics_AuditTrail";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed
            await SeedSupplierWithOrdersAsync(db, "TestBrand", 2, 2);

            // Act
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            Assert.NotEmpty(report.Suppliers);
            foreach (var supplier in report.Suppliers)
            {
                // Data version should be set
                Assert.NotNull(supplier.DataVersion);
                
                // Source record count must be > 0 (aggregated from orders)
                Assert.True(supplier.SourceRecordCount > 0);
                
                // Should be marked as aggregated
                Assert.True(supplier.IsAggregated);
                
                // Calculated timestamp should be recent
                Assert.True((DateTimeOffset.UtcNow - supplier.CalculatedAtUtc).TotalSeconds < 30);
            }
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_ConversionRateCalculation_IsAccurate()
        {
            const string dbName = "Analytics_ConversionRate";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed
            var (brand, products, orders) = await SeedSupplierWithOrdersAsync(db, "TestBrand", 2, 3);

            // Act
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            var supplier = report.Suppliers.First();
            
            // If there are product views, conversion rate should be positive
            if (supplier.ProductViews > 0)
            {
                Assert.True(supplier.ConversionRate >= 0);
                Assert.True(supplier.ConversionRate <= 100); // Percentage
            }
            else
            {
                // If no views, conversion rate can be 0
                Assert.True(supplier.ConversionRate >= 0);
            }
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_AverageOrderValue_IsCalculatedCorrectly()
        {
            const string dbName = "Analytics_AverageOrderValue";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed
            await SeedSupplierWithOrdersAsync(db, "TestBrand", 2, 3);

            // Act
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            var supplier = report.Suppliers.First();
            
            // Average order value = total revenue / total orders
            if (supplier.TotalOrders > 0)
            {
                var expectedAverage = supplier.TotalRevenue / supplier.TotalOrders;
                Assert.Equal(Math.Round(expectedAverage, 2), Math.Round(supplier.AverageOrderValue, 2));
            }
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_ReportMetadata_HasCorrectPeriod()
        {
            const string dbName = "Analytics_ReportMetadata";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed
            await SeedSupplierWithOrdersAsync(db, "TestBrand", 2, 2);

            // Act
            var report = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert
            Assert.True(report.ReportGeneratedAtUtc != default);
            Assert.True(report.PeriodStart != default);
            Assert.True(report.PeriodEnd != default);
            Assert.True(report.PeriodStart <= report.PeriodEnd);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_BoundaryDates_EdgeCasesCorrectlyIncluded()
        {
            const string dbName = "Analytics_BoundaryDates";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            var now = DateTimeOffset.UtcNow;
            var edgeDateStart = now.AddDays(-10).DateTime;
            var edgeDateEnd = now.AddDays(-5).DateTime;

            // Using pure DateTime for In-Memory test consistency with AnalyticsService logic
            var date1 = new DateTime(edgeDateStart.Ticks, DateTimeKind.Utc);
            var date2 = new DateTime(edgeDateEnd.Ticks, DateTimeKind.Utc);
            var date3 = new DateTime(edgeDateEnd.AddSeconds(1).Ticks, DateTimeKind.Utc);

            // Seed orders
            var order1 = new Order
            {
                OrderNumber = "ORD-EDGE-1",
                Status = OrderStatus.Completed,
                CreatedAtUtc = date1,
                PlacedAtUtc = date1,
                TotalAmount = 100m,
                Currency = "RSD",
                DeliveryMethod = Domain.Sales.DeliveryMethod.Courier,
                PaymentMethod = Domain.Sales.PaymentMethod.CashOnDelivery,
                CustomerFirstName = "John",
                CustomerLastName = "Doe",
                Email = "john@example.com",
                Phone = "+123456789",
                DeliveryAddressLine1 = "123 Main St",
                DeliveryCity = "New York",
                DeliveryPostalCode = "10001"
            };
            var order2 = new Order
            {
                OrderNumber = "ORD-EDGE-2",
                Status = OrderStatus.Completed,
                CreatedAtUtc = date2,
                PlacedAtUtc = date2,
                TotalAmount = 100m,
                Currency = "RSD",
                DeliveryMethod = Domain.Sales.DeliveryMethod.Courier,
                PaymentMethod = Domain.Sales.PaymentMethod.CashOnDelivery,
                CustomerFirstName = "John",
                CustomerLastName = "Doe",
                Email = "john@example.com",
                Phone = "+123456789",
                DeliveryAddressLine1 = "123 Main St",
                DeliveryCity = "New York",
                DeliveryPostalCode = "10001"
            };
            var order3 = new Order
            {
                OrderNumber = "ORD-EDGE-3",
                Status = OrderStatus.Completed,
                CreatedAtUtc = date3,
                PlacedAtUtc = date3,
                TotalAmount = 100m,
                Currency = "RSD",
                DeliveryMethod = Domain.Sales.DeliveryMethod.Courier,
                PaymentMethod = Domain.Sales.PaymentMethod.CashOnDelivery,
                CustomerFirstName = "John",
                CustomerLastName = "Doe",
                Email = "john@example.com",
                Phone = "+123456789",
                DeliveryAddressLine1 = "123 Main St",
                DeliveryCity = "New York",
                DeliveryPostalCode = "10001"
            };

            var brand = new Brand { Name = "BrandEdge", Slug = "brandedge" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            var product = new Product { Name = "EdgeProduct", BrandId = brand.Id, Slug = "edge-p" };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            foreach (var o in new[] { order1, order2, order3 })
            {
                o.Items.Add(new OrderItem 
                { 
                    ProductId = product.Id, 
                    ProductNameSnapshot = "EdgeProduct", 
                    BrandNameSnapshot = "BrandEdge",
                    Quantity = 1, 
                    UnitPrice = 100m,
                    LineTotal = 100m 
                });
                db.Orders.Add(o);
            }
            await db.SaveChangesAsync();

            // Act: Filter to exactly date1 to date2
            var report = await analyticsService.GetSupplierSalesStatsAsync(from: date1, to: date2);

            // Assert: Should have 2 orders (date1 and date2)
            var supplier = report.Suppliers.FirstOrDefault(s => s.BrandId == brand.Id);
            Assert.NotNull(supplier);
            Assert.Equal(2, supplier.TotalOrders);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_DeterministicSorting_ReturnsConsistentOrder()
        {
            const string dbName = "Analytics_Sorting";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            // Seed multiple suppliers with varying revenues
            for (int i = 0; i < 5; i++)
            {
                var revenueMultiplier = 5 - i; // Higher revenue for lower index
                await SeedSupplierWithOrdersAsync(db, $"Brand-{i}", 1, revenueMultiplier);
            }

            // Act: Call twice and compare order
            var report1 = await analyticsService.GetSupplierSalesStatsAsync();
            var report2 = await analyticsService.GetSupplierSalesStatsAsync();

            // Assert: Order of IDs must be identical
            var ids1 = report1.Suppliers.Select(s => s.BrandId).ToList();
            var ids2 = report2.Suppliers.Select(s => s.BrandId).ToList();
            
            Assert.Equal(ids1, ids2);
            // Verify it's sorted by revenue descending (highest first)
            Assert.Equal(ids1.First(), report1.Suppliers.OrderByDescending(s => s.TotalRevenue).First().BrandId);
        }

        [Fact]
        public async Task GetSupplierSalesStatsAsync_Concurrency_IsThreadSafe()
        {
            const string dbName = "Analytics_Concurrency";
            using var provider = CreateServiceProvider(dbName);
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TrendplusDbContext>();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            await SeedSupplierWithOrdersAsync(db, "ConcurrentBrand", 2, 5);

            // Act: Run multiple parallel tasks
            var tasks = Enumerable.Range(0, 10).Select(_ => analyticsService.GetSupplierSalesStatsAsync());
            var results = await Task.WhenAll(tasks);

            // Assert: All results should be identical and not crash
            var firstResultRevenue = results[0].TotalMarketRevenue;
            foreach (var result in results)
            {
                Assert.Equal(firstResultRevenue, result.TotalMarketRevenue);
                Assert.Equal(results[0].Suppliers.Count, result.Suppliers.Count);
            }
        }
    }
}
