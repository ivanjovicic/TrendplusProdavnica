using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.EnsureSchema(
                name: "pricing");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "brand_page_contents",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    HeroTitle = table.Column<string>(type: "text", nullable: true),
                    HeroSubtitle = table.Column<string>(type: "text", nullable: true),
                    IntroTitle = table.Column<string>(type: "text", nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    SeoText = table.Column<string>(type: "text", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    FeaturedLinks = table.Column<string>(type: "jsonb", nullable: true),
                    MerchBlocks = table.Column<string>(type: "jsonb", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand_page_contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LongDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CartToken = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RSD"),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    MenuLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Depth = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category_page_contents",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    HeroTitle = table.Column<string>(type: "text", nullable: true),
                    HeroSubtitle = table.Column<string>(type: "text", nullable: true),
                    IntroTitle = table.Column<string>(type: "text", nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    SeoText = table.Column<string>(type: "text", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    FeaturedLinks = table.Column<string>(type: "jsonb", nullable: true),
                    MerchBlocks = table.Column<string>(type: "jsonb", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_page_contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "collection_page_contents",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    HeroTitle = table.Column<string>(type: "text", nullable: true),
                    HeroSubtitle = table.Column<string>(type: "text", nullable: true),
                    IntroTitle = table.Column<string>(type: "text", nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    SeoText = table.Column<string>(type: "text", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    FeaturedLinks = table.Column<string>(type: "jsonb", nullable: true),
                    MerchBlocks = table.Column<string>(type: "jsonb", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection_page_contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Excerpt = table.Column<string>(type: "text", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    ReadingTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "home_pages",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "/"),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    Modules = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavigationMenus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    SizeGuideId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: false),
                    LongDescription = table.Column<string>(type: "text", nullable: true),
                    PrimaryColorName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    StyleTag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    OccasionTag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SeasonTag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPurchasable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBestseller = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortRank = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SearchKeywords = table.Column<string>(type: "text", nullable: true),
                    SearchSynonyms = table.Column<string[]>(type: "text[]", nullable: true),
                    SearchHidden = table.Column<bool>(type: "boolean", nullable: false),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DiscountType = table.Column<short>(type: "smallint", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AppliesToSalePrice = table.Column<bool>(type: "boolean", nullable: false),
                    BadgeText = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Priority = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    StartsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sale_pages",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    SeoText = table.Column<string>(type: "text", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DefaultSeoTitleSuffix = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DefaultOgImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupportEmail = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SupportPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SocialLinks = table.Column<string>(type: "jsonb", nullable: true),
                    ContactInfo = table.Column<string>(type: "jsonb", nullable: true),
                    AnalyticsSettings = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "size_guides",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_size_guides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlugRedirects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<short>(type: "smallint", nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: true),
                    OldPath = table.Column<string>(type: "text", nullable: false),
                    NewPath = table.Column<string>(type: "text", nullable: false),
                    StatusCode = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlugRedirects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "store_page_contents",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    HeroTitle = table.Column<string>(type: "text", nullable: true),
                    HeroSubtitle = table.Column<string>(type: "text", nullable: true),
                    IntroTitle = table.Column<string>(type: "text", nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    SeoText = table.Column<string>(type: "text", nullable: true),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    FeaturedLinks = table.Column<string>(type: "jsonb", nullable: true),
                    MerchBlocks = table.Column<string>(type: "jsonb", nullable: true),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_page_contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stores",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MallName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    WorkingHoursText = table.Column<string>(type: "text", nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    DirectionsUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrustPages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageKind = table.Column<short>(type: "smallint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "collections",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CollectionType = table.Column<short>(type: "smallint", nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LongDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BadgeText = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Seo_SeoTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_SeoDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_CanonicalUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_RobotsDirective = table.Column<string>(type: "text", nullable: true),
                    Seo_OgTitle = table.Column<string>(type: "text", nullable: true),
                    Seo_OgDescription = table.Column<string>(type: "text", nullable: true),
                    Seo_OgImageUrl = table.Column<string>(type: "text", nullable: true),
                    Seo_StructuredDataOverrideJson = table.Column<string>(type: "text", nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_collections_brands_BrandId",
                        column: x => x.BrandId,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticleBrand",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EditorialArticleId = table.Column<long>(type: "bigint", nullable: false),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticleBrand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorialArticleBrand_EditorialArticles_EditorialArticleId",
                        column: x => x.EditorialArticleId,
                        principalTable: "EditorialArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticleCategory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EditorialArticleId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticleCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorialArticleCategory_EditorialArticles_EditorialArticle~",
                        column: x => x.EditorialArticleId,
                        principalTable: "EditorialArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticleCollection",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EditorialArticleId = table.Column<long>(type: "bigint", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticleCollection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorialArticleCollection_EditorialArticles_EditorialArtic~",
                        column: x => x.EditorialArticleId,
                        principalTable: "EditorialArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticleProduct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EditorialArticleId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticleProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorialArticleProduct_EditorialArticles_EditorialArticleId",
                        column: x => x.EditorialArticleId,
                        principalTable: "EditorialArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NavigationMenuItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Badge = table.Column<string>(type: "text", nullable: true),
                    OpensInNewTab = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavigationMenuItems_NavigationMenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "NavigationMenuItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NavigationMenuItems_NavigationMenus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "NavigationMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_category_map",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_category_map", x => new { x.ProductId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_product_category_map_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_category_map_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_media",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    VariantId = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    MobileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AltText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    MediaType = table.Column<short>(type: "smallint", nullable: false),
                    MediaRole = table.Column<short>(type: "smallint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_media_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_related_products",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedProductId = table.Column<long>(type: "bigint", nullable: false),
                    RelationType = table.Column<short>(type: "smallint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_related_products", x => new { x.ProductId, x.RelatedProductId, x.RelationType });
                    table.ForeignKey(
                        name: "FK_product_related_products_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_related_products_products_RelatedProductId",
                        column: x => x.RelatedProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SizeEu = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    ColorName = table.Column<string>(type: "text", nullable: true),
                    ColorCode = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RSD"),
                    StockStatus = table.Column<short>(type: "smallint", nullable: false),
                    TotalStock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionBrands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionId = table.Column<long>(type: "bigint", nullable: false),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionBrands_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionCategories_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionCollections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionId = table.Column<long>(type: "bigint", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionCollections_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionProducts_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "size_guide_rows",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SizeGuideId = table.Column<long>(type: "bigint", nullable: false),
                    EuSize = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    FootLengthMinMm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FootLengthMaxMm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Note = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_size_guide_rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_size_guide_rows_size_guides_SizeGuideId",
                        column: x => x.SizeGuideId,
                        principalSchema: "catalog",
                        principalTable: "size_guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_collection_map",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Pinned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MerchandisingScore = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_collection_map", x => new { x.ProductId, x.CollectionId });
                    table.ForeignKey(
                        name: "FK_product_collection_map_collections_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "catalog",
                        principalTable: "collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_collection_map_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CartId = table.Column<long>(type: "bigint", nullable: false),
                    ProductVariantId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.Id);
                    table.CheckConstraint("ck_cart_items_quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("ck_cart_items_unit_price", "\"UnitPrice\" > 0");
                    table.ForeignKey(
                        name: "FK_cart_items_carts_CartId",
                        column: x => x.CartId,
                        principalSchema: "sales",
                        principalTable: "carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_product_variants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalSchema: "catalog",
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "store_inventory",
                schema: "inventory",
                columns: table => new
                {
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    VariantId = table.Column<long>(type: "bigint", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReservedQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_inventory", x => new { x.StoreId, x.VariantId });
                    table.ForeignKey(
                        name: "FK_store_inventory_product_variants_VariantId",
                        column: x => x.VariantId,
                        principalSchema: "catalog",
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_store_inventory_stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "inventory",
                        principalTable: "stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "categories",
                columns: new[] { "Id", "CreatedAtUtc", "ImageUrl", "IsActive", "MenuLabel", "Name", "ParentId", "ShortDescription", "Slug", "Type", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, null, "Cipele", null, null, "cipele", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 2L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, null, "Patike", null, null, "patike", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 3L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, null, "Čizme", null, null, "cizme", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 4L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, null, "Sandale", null, null, "sandale", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 5L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, null, "Papuče", null, null, "papuce", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "categories",
                columns: new[] { "Id", "CreatedAtUtc", "Depth", "ImageUrl", "IsActive", "MenuLabel", "Name", "ParentId", "ShortDescription", "Slug", "Type", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 101L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, null, true, null, "Salonke", 1L, null, "salonke", (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 102L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, null, true, null, "Baletanke", 1L, null, "baletanke", (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 103L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, null, true, null, "Mokasine", 1L, null, "mokasine", (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 104L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, null, true, null, "Gležnjače", 3L, null, "gleznjace", (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 105L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), (short)1, null, true, null, "Lifestyle", 2L, null, "lifestyle", (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "ux_brands_slug",
                schema: "catalog",
                table: "brands",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id",
                schema: "sales",
                table: "cart_items",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_product_variant_id",
                schema: "sales",
                table: "cart_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "ux_cart_items_cart_id_product_variant_id",
                schema: "sales",
                table: "cart_items",
                columns: new[] { "CartId", "ProductVariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carts_expires_at_utc",
                schema: "sales",
                table: "carts",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "ix_carts_status",
                schema: "sales",
                table: "carts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ux_carts_cart_token",
                schema: "sales",
                table: "carts",
                column: "CartToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_active_sort_id",
                schema: "catalog",
                table: "categories",
                columns: new[] { "IsActive", "SortOrder", "Id" })
                .Annotation("Npgsql:IndexInclude", new[] { "Name", "Slug", "MenuLabel" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_sort_id",
                schema: "catalog",
                table: "categories",
                columns: new[] { "ParentId", "SortOrder", "Id" });

            migrationBuilder.CreateIndex(
                name: "ux_categories_slug",
                schema: "catalog",
                table: "categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collections_BrandId",
                schema: "catalog",
                table: "collections",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "ux_collections_slug",
                schema: "catalog",
                table: "collections",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticleBrand_EditorialArticleId",
                table: "EditorialArticleBrand",
                column: "EditorialArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticleCategory_EditorialArticleId",
                table: "EditorialArticleCategory",
                column: "EditorialArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticleCollection_EditorialArticleId",
                table: "EditorialArticleCollection",
                column: "EditorialArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticleProduct_EditorialArticleId",
                table: "EditorialArticleProduct",
                column: "EditorialArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationMenuItems_MenuId",
                table: "NavigationMenuItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationMenuItems_ParentId",
                table: "NavigationMenuItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_map_CategoryId",
                schema: "catalog",
                table: "product_category_map",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_product_collection_map_CollectionId",
                schema: "catalog",
                table: "product_collection_map",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "ix_product_media_product_sort_id",
                schema: "catalog",
                table: "product_media",
                columns: new[] { "ProductId", "SortOrder", "Id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_media_role_active_sort",
                schema: "catalog",
                table: "product_media",
                columns: new[] { "ProductId", "MediaRole", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_product_related_products_RelatedProductId",
                schema: "catalog",
                table: "product_related_products",
                column: "RelatedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId",
                schema: "catalog",
                table: "product_variants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "ux_product_variants_sku",
                schema: "catalog",
                table: "product_variants",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_products_slug",
                schema: "catalog",
                table: "products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBrands_PromotionId",
                table: "PromotionBrands",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCategories_PromotionId",
                table: "PromotionCategories",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCollections_PromotionId",
                table: "PromotionCollections",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionProducts_PromotionId",
                table: "PromotionProducts",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "ux_size_guide_rows_eu_size",
                schema: "catalog",
                table: "size_guide_rows",
                columns: new[] { "SizeGuideId", "EuSize" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_size_guides_slug",
                schema: "catalog",
                table: "size_guides",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_store_inventory_VariantId",
                schema: "inventory",
                table: "store_inventory",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "ux_stores_slug",
                schema: "inventory",
                table: "stores",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brand_page_contents",
                schema: "content");

            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "category_page_contents",
                schema: "content");

            migrationBuilder.DropTable(
                name: "collection_page_contents",
                schema: "content");

            migrationBuilder.DropTable(
                name: "EditorialArticleBrand");

            migrationBuilder.DropTable(
                name: "EditorialArticleCategory");

            migrationBuilder.DropTable(
                name: "EditorialArticleCollection");

            migrationBuilder.DropTable(
                name: "EditorialArticleProduct");

            migrationBuilder.DropTable(
                name: "home_pages",
                schema: "content");

            migrationBuilder.DropTable(
                name: "NavigationMenuItems");

            migrationBuilder.DropTable(
                name: "product_category_map",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_collection_map",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_media",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_related_products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "PromotionBrands");

            migrationBuilder.DropTable(
                name: "PromotionCategories");

            migrationBuilder.DropTable(
                name: "PromotionCollections");

            migrationBuilder.DropTable(
                name: "PromotionProducts");

            migrationBuilder.DropTable(
                name: "sale_pages",
                schema: "content");

            migrationBuilder.DropTable(
                name: "site_settings",
                schema: "content");

            migrationBuilder.DropTable(
                name: "size_guide_rows",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SlugRedirects");

            migrationBuilder.DropTable(
                name: "store_inventory",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "store_page_contents",
                schema: "content");

            migrationBuilder.DropTable(
                name: "TrustPages");

            migrationBuilder.DropTable(
                name: "carts",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "EditorialArticles");

            migrationBuilder.DropTable(
                name: "NavigationMenus");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "collections",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "promotions",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "size_guides",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_variants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "stores",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "brands",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");
        }
    }
}
