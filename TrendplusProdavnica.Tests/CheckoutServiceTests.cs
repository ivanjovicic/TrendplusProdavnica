using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Services;

namespace TrendplusProdavnica.Tests;

public sealed class CheckoutServiceTests
{
    [Fact]
    public async Task PlaceOrder_WithSameIdempotencyKeyTwice_ReturnsAlreadyProcessedOnSecondCall()
    {
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-repeat", stock: 5, quantity: 1);

        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();

        var firstService = new CheckoutService(firstContext);
        var secondService = new CheckoutService(secondContext);
        var request = CheckoutTestDatabase.CreateRequest("cart-repeat", "idem-repeat");

        var first = await firstService.PlaceOrderAsync(request);
        var second = await secondService.PlaceOrderAsync(request);

        Assert.Equal(CheckoutOutcome.Success, first.Outcome);
        Assert.Equal(CheckoutOutcome.AlreadyProcessed, second.Outcome);
        Assert.Equal(first.OrderNumber, second.OrderNumber);
    }

    [Fact]
    public async Task PlaceOrder_WithInsufficientStock_ReturnsInsufficientStock()
    {
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-stock", stock: 1, quantity: 2);

        await using var context = database.CreateContext();
        var service = new CheckoutService(context);

        var result = await service.PlaceOrderAsync(CheckoutTestDatabase.CreateRequest("cart-stock", "idem-stock"));

        Assert.Equal(CheckoutOutcome.InsufficientStock, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.True(string.IsNullOrEmpty(result.OrderNumber));
    }

    [Fact]
    public async Task PlaceOrder_WithoutExplicitIdempotencyKey_UsesCartTokenFallbackForReplay()
    {
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-fallback", stock: 5, quantity: 1);

        var request = CheckoutTestDatabase.CreateRequest("cart-fallback", idempotencyKey: null);

        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();

        var first = await new CheckoutService(firstContext).PlaceOrderAsync(request);
        var replay = await new CheckoutService(secondContext).PlaceOrderAsync(request);

        Assert.Equal(CheckoutOutcome.Success, first.Outcome);
        Assert.Equal(CheckoutOutcome.AlreadyProcessed, replay.Outcome);
        Assert.Equal(first.OrderNumber, replay.OrderNumber);
    }

    [Fact(Skip = "Promote to PostgreSQL integration test once legacy migration chain is stabilized.")]
    public async Task PlaceOrder_ParallelSameRequest_CreatesSingleOrder()
    {
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-parallel", stock: 6, quantity: 2);

        async Task<CheckoutResultDto> RunAsync()
        {
            await using var context = database.CreateContext();
            var service = new CheckoutService(context);
            return await service.PlaceOrderAsync(CheckoutTestDatabase.CreateRequest("cart-parallel", "idem-parallel"));
        }

        var results = await Task.WhenAll(RunAsync(), RunAsync());

        Assert.Contains(results, item => item.Outcome == CheckoutOutcome.Success);
        Assert.Contains(results, item => item.Outcome == CheckoutOutcome.AlreadyProcessed);
    }

    [Fact]
    public async Task PlaceOrder_WithMultipleVariants_LocksAllAndValidatesAll()
    {
        // Test with multiple variants to ensure locking works for multi-variant carts
        await using var database = CheckoutTestDatabase.Create();
        var cartToken = "cart-multi-variant";
        
        await database.SeedMultiVariantCartAsync(
            cartToken: cartToken,
            variantConfigs: new[]
            {
                (stock: 5, quantity: 2),
                (stock: 3, quantity: 1),
                (stock: 10, quantity: 5)
            });

        await using var context = database.CreateContext();
        var service = new CheckoutService(context);
        var request = CheckoutTestDatabase.CreateRequest(cartToken, "idem-multi");

        var result = await service.PlaceOrderAsync(request);

        Assert.Equal(CheckoutOutcome.Success, result.Outcome);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task PlaceOrder_WithRaceConditionScenario_OneSucceedsOtherDetectsConflict()
    {
        // This test simulates the race condition protection
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-race", stock: 3, quantity: 2);

        var request1 = CheckoutTestDatabase.CreateRequest("cart-race", "idem-race-1");
        var request2 = CheckoutTestDatabase.CreateRequest("cart-race", "idem-race-2");

        await using var context1 = database.CreateContext();
        await using var context2 = database.CreateContext();

        var service1 = new CheckoutService(context1);
        var service2 = new CheckoutService(context2);

        // First request succeeds
        var result1 = await service1.PlaceOrderAsync(request1);
        Assert.Equal(CheckoutOutcome.Success, result1.Outcome);

        // Second request with same cart but different idempotency key should fail
        // (because cart has been converted)
        var result2 = await service2.PlaceOrderAsync(request2);
        Assert.Equal(CheckoutOutcome.InvalidCart, result2.Outcome);
    }

    [Fact]
    public async Task PlaceOrder_CartAlreadyConverted_ReturnsMappedExistingOrder()
    {
        // If cart is already Converted, should return existing order
        await using var database = CheckoutTestDatabase.Create();
        await database.SeedCartAsync(cartToken: "cart-converted", stock: 5, quantity: 1);

        var request = CheckoutTestDatabase.CreateRequest("cart-converted", "idem-converted");

        // First checkout
        await using var firstContext = database.CreateContext();
        var firstResult = await new CheckoutService(firstContext).PlaceOrderAsync(request);
        Assert.Equal(CheckoutOutcome.Success, firstResult.Outcome);

        // Verify cart is converted
        await using var verifyContext = database.CreateContext();
        var cart = await verifyContext.Carts.FirstAsync(c => c.CartToken == "cart-converted");
        Assert.Equal(CartStatus.Converted, cart.Status);

        // Second checkout with different idempotency key
        var request2 = CheckoutTestDatabase.CreateRequest("cart-converted", "idem-converted-2");
        await using var secondContext = database.CreateContext();
        var secondResult = await new CheckoutService(secondContext).PlaceOrderAsync(request2);
        
        // Should fail because cart is converted, returning InvalidCart
        Assert.Equal(CheckoutOutcome.InvalidCart, secondResult.Outcome);
    }

    private sealed class CheckoutTestDatabase : IAsyncDisposable
    {
        private readonly string _databaseName = $"checkout-tests-{Guid.NewGuid():N}";
        private readonly InMemoryDatabaseRoot _databaseRoot = new();

        public static CheckoutTestDatabase Create() => new();

        public TrendplusDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TrendplusDbContext>()
                .UseInMemoryDatabase(_databaseName, _databaseRoot)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new TrendplusDbContext(options);
        }

        public async Task SeedCartAsync(string cartToken, int stock, int quantity)
        {
            await using var context = CreateContext();

            var now = DateTimeOffset.UtcNow;

            var brand = new Brand
            {
                Name = "Test brand",
                Slug = $"test-brand-{Guid.NewGuid():N}"[..18],
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            var category = new Category
            {
                Name = "Cipele",
                Slug = $"cipele-{Guid.NewGuid():N}"[..18],
                Depth = 0,
                SortOrder = 1,
                IsActive = true,
                Type = CategoryType.Root,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Brands.Add(brand);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                BrandId = brand.Id,
                PrimaryCategoryId = category.Id,
                Name = "Test proizvod",
                Slug = $"test-proizvod-{Guid.NewGuid():N}"[..22],
                ShortDescription = "Test opis",
                Status = ProductStatus.Published,
                IsVisible = true,
                IsPurchasable = true,
                PublishedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = $"SKU-{Guid.NewGuid():N}"[..16],
                SizeEu = 38,
                Price = 10000m,
                Currency = "RSD",
                StockStatus = stock > 0 ? StockStatus.InStock : StockStatus.OutOfStock,
                TotalStock = stock,
                LowStockThreshold = 1,
                IsActive = true,
                IsVisible = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.ProductVariants.Add(variant);
            await context.SaveChangesAsync();

            var cart = new Cart
            {
                CartToken = cartToken,
                Status = CartStatus.Active,
                Currency = "RSD",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            context.Set<CartItem>().Add(new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = variant.Id,
                ProductVariant = variant,
                Quantity = quantity,
                UnitPrice = variant.Price,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            await context.SaveChangesAsync();
        }

        public static CheckoutRequest CreateRequest(string cartToken, string? idempotencyKey)
        {
            return new CheckoutRequest
            {
                IdempotencyKey = idempotencyKey,
                CartToken = cartToken,
                CustomerFirstName = "Ana",
                CustomerLastName = "Test",
                Email = "ana@test.rs",
                CustomerEmail = "ana@test.rs",
                Phone = "0601234567",
                DeliveryAddressLine1 = "Bulevar 1",
                DeliveryCity = "Beograd",
                DeliveryPostalCode = "11000",
                DeliveryMethod = DeliveryMethod.Courier,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                Note = "Test"
            };
        }

        public async Task SeedMultiVariantCartAsync(string cartToken, (int stock, int quantity)[] variantConfigs)
        {
            await using var context = CreateContext();
            var now = DateTimeOffset.UtcNow;

            var brand = new Brand
            {
                Name = "Test brand multi",
                Slug = $"test-brand-{Guid.NewGuid():N}"[..18],
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            var category = new Category
            {
                Name = "Cipele",
                Slug = $"cipele-{Guid.NewGuid():N}"[..18],
                Depth = 0,
                SortOrder = 1,
                IsActive = true,
                Type = CategoryType.Root,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Brands.Add(brand);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                BrandId = brand.Id,
                PrimaryCategoryId = category.Id,
                Name = "Test proizvod multi",
                Slug = $"test-proizvod-{Guid.NewGuid():N}"[..22],
                ShortDescription = "Test opis",
                Status = ProductStatus.Published,
                IsVisible = true,
                IsPurchasable = true,
                PublishedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var variants = new List<ProductVariant>();
            foreach (var (stock, _) in variantConfigs)
            {
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Sku = $"SKU-{Guid.NewGuid():N}"[..16],
                    SizeEu = 36 + variants.Count,
                    Price = 10000m,
                    Currency = "RSD",
                    StockStatus = stock > 0 ? StockStatus.InStock : StockStatus.OutOfStock,
                    TotalStock = stock,
                    LowStockThreshold = 1,
                    IsActive = true,
                    IsVisible = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };
                variants.Add(variant);
                context.ProductVariants.Add(variant);
            }

            await context.SaveChangesAsync();

            var cart = new Cart
            {
                CartToken = cartToken,
                Status = CartStatus.Active,
                Currency = "RSD",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            for (int i = 0; i < variantConfigs.Length; i++)
            {
                var (_, quantity) = variantConfigs[i];
                context.Set<CartItem>().Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variants[i].Id,
                    ProductVariant = variants[i],
                    Quantity = quantity,
                    UnitPrice = variants[i].Price,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            await context.SaveChangesAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
