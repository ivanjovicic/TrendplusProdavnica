#nullable enable
using System;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.Analytics;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Experiments;
using TrendplusProdavnica.Domain.Inventory;
using TrendplusProdavnica.Domain.Merchandising;
using TrendplusProdavnica.Domain.Personalization;
using TrendplusProdavnica.Domain.Pricing;
using TrendplusProdavnica.Domain.Shared;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Domain.Search;

namespace TrendplusProdavnica.Infrastructure.Persistence
{
    public class TrendplusDbContext : DbContext
    {
        public TrendplusDbContext(DbContextOptions<TrendplusDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<SizeGuide> SizeGuides { get; set; } = null!;
        public DbSet<SizeGuideRow> SizeGuideRows { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductMedia> ProductMedia { get; set; } = null!;
        public DbSet<ProductReview> ProductReviews { get; set; } = null!;
        public DbSet<ProductRating> ProductRatings { get; set; } = null!;
        public DbSet<ProductCategoryMap> ProductCategoryMaps { get; set; } = null!;
        public DbSet<ProductCollectionMap> ProductCollectionMaps { get; set; } = null!;
        public DbSet<ProductRelatedProduct> ProductRelatedProducts { get; set; } = null!;

        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<StoreInventory> StoreInventory { get; set; } = null!;

        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<PromotionProduct> PromotionProducts { get; set; } = null!;
        public DbSet<PromotionCategory> PromotionCategories { get; set; } = null!;
        public DbSet<PromotionBrand> PromotionBrands { get; set; } = null!;
        public DbSet<PromotionCollection> PromotionCollections { get; set; } = null!;

        public DbSet<MerchandisingRule> MerchandisingRules { get; set; } = null!;

        public DbSet<SiteSettings> SiteSettings { get; set; } = null!;
        public DbSet<NavigationMenu> NavigationMenus { get; set; } = null!;
        public DbSet<NavigationMenuItem> NavigationMenuItems { get; set; } = null!;
        public DbSet<HomePage> HomePages { get; set; } = null!;
        public DbSet<CategoryPageContent> CategoryPageContents { get; set; } = null!;
        public DbSet<BrandPageContent> BrandPageContents { get; set; } = null!;
        public DbSet<CollectionPageContent> CollectionPageContents { get; set; } = null!;
        public DbSet<StorePageContent> StorePageContents { get; set; } = null!;
        public DbSet<SalePage> SalePages { get; set; } = null!;
        public DbSet<EditorialArticle> EditorialArticles { get; set; } = null!;
        public DbSet<TrustPage> TrustPages { get; set; } = null!;

        public DbSet<SlugRedirect> SlugRedirects { get; set; } = null!;

        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        public DbSet<Wishlist> Wishlists { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;

        public DbSet<SearchIndexEventLog> SearchIndexEventLogs { get; set; } = null!;

        public DbSet<CategorySeoContent> CategorySeoContents { get; set; } = null!;

        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; } = null!;

        public DbSet<Experiment> Experiments { get; set; } = null!;
        public DbSet<ExperimentAssignment> ExperimentAssignments { get; set; } = null!;

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Owned<TrendplusProdavnica.Domain.ValueObjects.SeoMetadata>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrendplusDbContext).Assembly);

            var seededAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            modelBuilder.Entity<Category>().HasData(
                new { Id = 1L, Name = "Cipele", Slug = "cipele", Depth = (short)0, SortOrder = 0, IsActive = true, Type = CategoryType.Root, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 2L, Name = "Patike", Slug = "patike", Depth = (short)0, SortOrder = 0, IsActive = true, Type = CategoryType.Root, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 3L, Name = "\u010Cizme", Slug = "cizme", Depth = (short)0, SortOrder = 0, IsActive = true, Type = CategoryType.Root, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 4L, Name = "Sandale", Slug = "sandale", Depth = (short)0, SortOrder = 0, IsActive = true, Type = CategoryType.Root, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 5L, Name = "Papu\u010De", Slug = "papuce", Depth = (short)0, SortOrder = 0, IsActive = true, Type = CategoryType.Root, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 101L, ParentId = 1L, Name = "Salonke", Slug = "salonke", Depth = (short)1, SortOrder = 0, IsActive = true, Type = CategoryType.Subcategory, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 102L, ParentId = 1L, Name = "Baletanke", Slug = "baletanke", Depth = (short)1, SortOrder = 0, IsActive = true, Type = CategoryType.Subcategory, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 103L, ParentId = 1L, Name = "Mokasine", Slug = "mokasine", Depth = (short)1, SortOrder = 0, IsActive = true, Type = CategoryType.Subcategory, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 104L, ParentId = 3L, Name = "Gle\u017Enja\u010De", Slug = "gleznjace", Depth = (short)1, SortOrder = 0, IsActive = true, Type = CategoryType.Subcategory, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc },
                new { Id = 105L, ParentId = 2L, Name = "Lifestyle", Slug = "lifestyle", Depth = (short)1, SortOrder = 0, IsActive = true, Type = CategoryType.Subcategory, CreatedAtUtc = seededAtUtc, UpdatedAtUtc = seededAtUtc }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
