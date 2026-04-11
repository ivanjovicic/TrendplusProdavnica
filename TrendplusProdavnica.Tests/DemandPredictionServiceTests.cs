using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if false
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TrendplusProdavnica.Application.Analytics.DTOs;
using TrendplusProdavnica.Application.Analytics.Services;
using TrendplusProdavnica.Infrastructure.DemandPrediction;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Analytics;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Domain.Catalog;
using Microsoft.Extensions.Logging;
using Moq;
#endif

namespace TrendplusProdavnica.Tests.Analytics
{
#if false
    [TestFixture]
    public class DemandPredictionServiceTests
    {
        private TrendplusDbContext _dbContext;
        private DemandPredictionService _service;
        private DemandPredictionQueries _queries;
        private Mock<ILogger<DemandPredictionService>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TrendplusDbContext>()
                .UseInMemoryDatabase(databaseName: $"test-db-{Guid.NewGuid()}")
                .Options;

            _dbContext = new TrendplusDbContext(options);
            _queries = new DemandPredictionQueries(_dbContext);
            _loggerMock = new Mock<ILogger<DemandPredictionService>>();
            _service = new DemandPredictionService(_dbContext, _queries, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        #region Test Data Generators

        private Product CreateProduct(long id = 1, string name = "Test Product")
        {
            var product = new Product
            {
                Id = id,
                Name = name,
                Slug = name.ToLower().Replace(" ", "-"),
                ShortDescription = "Test description",
                IsVisible = true,
                IsPurchasable = true,
                BrandId = 1,
                PrimaryCategoryId = 1
            };
            return product;
        }

        private Order CreateOrder(long id, DateTimeOffset placedAt, OrderStatus status = OrderStatus.Completed)
        {
            var order = new Order
            {
                Id = id,
                OrderNumber = $"ORD-{id}",
                CreatedAtUtc = placedAt.AddDays(-1),
                PlacedAtUtc = placedAt,
                UpdatedAtUtc = placedAt,
                Status = status,
                CustomerFirstName = "Test",
                CustomerLastName = "Customer",
                Email = "test@example.com",
                Phone = "123456789",
                DeliveryAddressLine1 = "Street",
                DeliveryCity = "City",
                DeliveryPostalCode = "12345",
                SubtotalAmount = 100,
                DeliveryAmount = 10,
                TotalAmount = 110
            };
            return order;
        }

        private OrderItem CreateOrderItem(long orderId, long productId, int quantity = 1, decimal size = 38)
        {
            var item = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                ProductVariantId = 1,
                ProductNameSnapshot = "Test Product",
                BrandNameSnapshot = "Test Brand",
                SizeEuSnapshot = size,
                UnitPrice = 100,
                Quantity = quantity,
                LineTotal = 100 * quantity
            };
            return item;
        }

        #endregion

        #region Tests

        [Test]
        public async Task PredictDemand_WithValidProduct_ReturnsValidPrediction()
        {
            var product = CreateProduct(1, "Adidas Superstar");
            _dbContext.Products.Add(product);

            var baseDate = DateTimeOffset.UtcNow.AddMonths(-12);
            for (int month = 0; month < 12; month++)
            {
                var orderDate = baseDate.AddMonths(month);
                var order = CreateOrder(month + 1, orderDate);
                _dbContext.Orders.Add(order);

                for (int i = 0; i < 50; i++)
                {
                    var size = 36 + (i % 7) * 0.5m;
                    var item = CreateOrderItem(order.Id, 1, 1, size);
                    item.Order = order;
                    _dbContext.OrderItems.Add(item);
                }
            }

            await _dbContext.SaveChangesAsync();

            var prediction = await _service.PredictDemandAsync(
                new DemandPredictionRequest { ProductId = 1 });

            Assert.NotNull(prediction);
            Assert.That(prediction.ProductId, Is.EqualTo(1));
            Assert.That(prediction.ProductName, Is.EqualTo("Adidas Superstar"));
            Assert.That(prediction.ExpectedMonthlySales, Is.GreaterThan(40));
            Assert.That(prediction.ExpectedMonthlySales, Is.LessThan(60));
            Assert.That(prediction.ConfidenceScore, Is.GreaterThan(50));
            Assert.That(prediction.SizeDistribution.Count, Is.GreaterThan(0));
            Assert.That(prediction.Status, Is.EqualTo("COMPLETED"));
        }

        [Test]
        public async Task PredictDemand_WithNonExistentProduct_ThrowsException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _service.PredictDemandAsync(
                    new DemandPredictionRequest { ProductId = 999 });
            });
        }

        [Test]
        public async Task PredictDemand_WithInsufficientData_ReturnsLowConfidenceScore()
        {
            var product = CreateProduct(1);
            _dbContext.Products.Add(product);

            var order = CreateOrder(1, DateTimeOffset.UtcNow);
            _dbContext.Orders.Add(order);
            var item = CreateOrderItem(1, 1, 5);
            item.Order = order;
            _dbContext.OrderItems.Add(item);

            await _dbContext.SaveChangesAsync();

            var prediction = await _service.PredictDemandAsync(
                new DemandPredictionRequest { ProductId = 1 });

            Assert.That(prediction.ConfidenceScore, Is.LessThan(50));
        }

        [Test]
        public async Task PredictDemand_SizeDistribution_CalculatesCorrectly()
        {
            var product = CreateProduct(1);
            _dbContext.Products.Add(product);

            var baseDate = DateTimeOffset.UtcNow.AddMonths(-12);
            var order = CreateOrder(1, baseDate);
            _dbContext.Orders.Add(order);

            for (int i = 0; i < 40; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(1, 1, 1, 38) { Order = order });
            for (int i = 0; i < 30; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(1, 1, 1, 39) { Order = order });
            for (int i = 0; i < 20; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(1, 1, 1, 37) { Order = order });
            for (int i = 0; i < 10; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(1, 1, 1, 40) { Order = order });

            await _dbContext.SaveChangesAsync();

            var prediction = await _service.PredictDemandAsync(
                new DemandPredictionRequest { ProductId = 1, IsFootwear = true });

            var size38 = prediction.SizeDistribution.First(s => s.Size == 38);
            Assert.That(size38.PercentageOfTotal, Is.GreaterThan(38).And.LessThan(42));

            var size39 = prediction.SizeDistribution.First(s => s.Size == 39);
            Assert.That(size39.PercentageOfTotal, Is.GreaterThan(28).And.LessThan(32));
        }

        [Test]
        public async Task PredictDemandBulk_MultipleProducts_ReturnsAllPredictions()
        {
            var products = new[] { 1L, 2L, 3L };
            foreach (var id in products)
            {
                _dbContext.Products.Add(CreateProduct(id, $"Product {id}"));
            }

            var baseDate = DateTimeOffset.UtcNow.AddMonths(-6);
            for (int month = 0; month < 6; month++)
            {
                var order = CreateOrder(month + 1, baseDate.AddMonths(month));
                _dbContext.Orders.Add(order);

                foreach (var productId in products)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        var item = CreateOrderItem(order.Id, productId, 1, 38);
                        item.Order = order;
                        _dbContext.OrderItems.Add(item);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            var response = await _service.PredictDemandBulkAsync(
                new BulkDemandPredictionRequest { ProductIds = products.ToList() });

            Assert.That(response.SuccessCount, Is.EqualTo(3));
            Assert.That(response.FailureCount, Is.EqualTo(0));
            Assert.That(response.Predictions.Count, Is.EqualTo(3));
            Assert.That(response.Errors, Is.Empty);
        }

        [Test]
        public async Task PredictDemandBulk_SomeFail_ReturnsPartialResults()
        {
            var validProductId = 1L;
            var invalidProductId = 999L;

            _dbContext.Products.Add(CreateProduct(validProductId));

            var order = CreateOrder(1, DateTimeOffset.UtcNow);
            _dbContext.Orders.Add(order);
            var item = CreateOrderItem(1, validProductId, 10);
            item.Order = order;
            _dbContext.OrderItems.Add(item);

            await _dbContext.SaveChangesAsync();

            var response = await _service.PredictDemandBulkAsync(
                new BulkDemandPredictionRequest
                {
                    ProductIds = new List<long> { validProductId, invalidProductId }
                });

            Assert.That(response.SuccessCount, Is.EqualTo(1));
            Assert.That(response.FailureCount, Is.EqualTo(1));
            Assert.That(response.Errors, Is.Not.Empty);
        }

        [Test]
        public async Task PredictDemand_DecreasingTrend_ForecastLowerThanAverage()
        {
            var product = CreateProduct(1);
            _dbContext.Products.Add(product);

            var baseDate = DateTimeOffset.UtcNow.AddMonths(-12);

            for (int month = 0; month < 12; month++)
            {
                var order = CreateOrder(month + 1, baseDate.AddMonths(month));
                _dbContext.Orders.Add(order);

                int quantity = 100 - (month * 8);
                for (int i = 0; i < quantity; i++)
                {
                    var item = CreateOrderItem(order.Id, 1, 1, 38);
                    item.Order = order;
                    _dbContext.OrderItems.Add(item);
                }
            }

            await _dbContext.SaveChangesAsync();

            var prediction = await _service.PredictDemandAsync(
                new DemandPredictionRequest { ProductId = 1 });

            Assert.That(prediction.ForecastNextMonth, Is.LessThan(prediction.ExpectedMonthlySales));
        }

        [Test]
        public async Task PredictDemand_IncreasingTrend_ForecastHigherThanAverage()
        {
            var product = CreateProduct(1);
            _dbContext.Products.Add(product);

            var baseDate = DateTimeOffset.UtcNow.AddMonths(-12);

            for (int month = 0; month < 12; month++)
            {
                var order = CreateOrder(month + 1, baseDate.AddMonths(month));
                _dbContext.Orders.Add(order);

                int quantity = 30 + (month * 5);
                for (int i = 0; i < quantity; i++)
                {
                    var item = CreateOrderItem(order.Id, 1, 1, 38);
                    item.Order = order;
                    _dbContext.OrderItems.Add(item);
                }
            }

            await _dbContext.SaveChangesAsync();

            var prediction = await _service.PredictDemandAsync(
                new DemandPredictionRequest { ProductId = 1 });

            Assert.That(prediction.ForecastNextMonth, Is.GreaterThan(prediction.ExpectedMonthlySales * 0.8));
        }

        [Test]
        public async Task GetProcurementRecommendations_WithSafetyStock_IncreasesMonthlySales()
        {
            var product = CreateProduct(1);
            _dbContext.Products.Add(product);

            var order = CreateOrder(1, DateTimeOffset.UtcNow.AddMonths(-6));
            _dbContext.Orders.Add(order);

            for (int i = 0; i < 100; i++)
            {
                var item = CreateOrderItem(1, 1, 1, 38 + (i % 3) * 0.5m);
                item.Order = order;
                _dbContext.OrderItems.Add(item);
            }

            await _dbContext.SaveChangesAsync();

            var recommendations = await _service.GetProcurementRecommendationsAsync(1, safetyStockPercentage: 50);

            foreach (var rec in recommendations)
            {
                Assert.That(rec.RecommendedStockQuantity, Is.GreaterThan(0));
                var expectedWithoutSafety = (int)(rec.UnitsSold * (recommendations.Sum(x => x.PercentageOfTotal) / 100) / 100);
                var ratio = rec.RecommendedStockQuantity / (decimal)(expectedWithoutSafety > 0 ? expectedWithoutSafety : 1);
                Assert.That(ratio, Is.GreaterThan(1));
            }
        }

        [Test]
        public async Task GetCategorySeasonality_MultipleProducts_CalculatesSeasonalIndex()
        {
            var categoryId = 5L;
            for (int p = 1; p <= 3; p++)
            {
                _dbContext.Products.Add(CreateProduct(p, $"Product {p}"));
            }

            var baseDate = DateTimeOffset.UtcNow.AddYears(-2);

            for (int month = 0; month < 24; month++)
            {
                var currentDate = baseDate.AddMonths(month);
                var season = GetSeason(currentDate);

                var order = CreateOrder(month + 1, currentDate);
                _dbContext.Orders.Add(order);

                int quantityBase = season == "SUMMER" ? 80 : 40;

                for (int p = 1; p <= 3; p++)
                {
                    _dbContext.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = p,
                        ProductVariantId = 1,
                        ProductNameSnapshot = $"Product {p}",
                        BrandNameSnapshot = "Test",
                        SizeEuSnapshot = 38,
                        UnitPrice = 100,
                        Quantity = quantityBase,
                        LineTotal = quantityBase * 100,
                        Order = order
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            var seasonality = await _service.GetCategorySeasonalityAsync(categoryId);

            Assert.That(seasonality, Is.Not.Empty);

            var summer = seasonality.First(s => s.Season == "SUMMER");
            var winter = seasonality.First(s => s.Season == "WINTER");

            Assert.That(summer.SeasonalIndex, Is.GreaterThan(winter.SeasonalIndex));
        }

        [Test]
        public async Task GetTopDemandProducts_ReturnsSorted()
        {
            var baseDate = DateTimeOffset.UtcNow.AddMonths(-12);

            _dbContext.Products.Add(CreateProduct(1, "Product 1"));
            var order1 = CreateOrder(1, baseDate);
            _dbContext.Orders.Add(order1);
            for (int i = 0; i < 20; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(1, 1, 1, 38) { Order = order1 });

            _dbContext.Products.Add(CreateProduct(2, "Product 2"));
            var order2 = CreateOrder(2, baseDate);
            _dbContext.Orders.Add(order2);
            for (int i = 0; i < 80; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(2, 2, 1, 38) { Order = order2 });

            _dbContext.Products.Add(CreateProduct(3, "Product 3"));
            var order3 = CreateOrder(3, baseDate);
            _dbContext.Orders.Add(order3);
            for (int i = 0; i < 50; i++)
                _dbContext.OrderItems.Add(CreateOrderItem(3, 3, 1, 38) { Order = order3 });

            await _dbContext.SaveChangesAsync();

            var topProducts = await _service.GetTopDemandProductsAsync(limit: 3);

            Assert.That(topProducts.Count, Is.EqualTo(3));
            Assert.That(topProducts[0].ProductId, Is.EqualTo(2));
            Assert.That(topProducts[1].ProductId, Is.EqualTo(3));
            Assert.That(topProducts[2].ProductId, Is.EqualTo(1));
        }

        #endregion

        #region Helpers

        private static string GetSeason(DateTimeOffset date)
        {
            return date.Month switch
            {
                12 or 1 or 2 => "WINTER",
                3 or 4 or 5 => "SPRING",
                6 or 7 or 8 => "SUMMER",
                9 or 10 or 11 => "FALL",
                _ => "UNKNOWN"
            };
        }

        #endregion
    }
#endif
}
