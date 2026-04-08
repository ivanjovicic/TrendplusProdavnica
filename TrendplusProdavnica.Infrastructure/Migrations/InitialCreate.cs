using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schemas
            migrationBuilder.EnsureSchema(name: "catalog");
            migrationBuilder.EnsureSchema(name: "inventory");
            migrationBuilder.EnsureSchema(name: "pricing");
            migrationBuilder.EnsureSchema(name: "content");
            migrationBuilder.EnsureSchema(name: "shared");

            // catalog.categories
            migrationBuilder.CreateTable(
                name: "categories",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    menu_label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    depth = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    type = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_parent",
                        column: x => x.parent_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_categories_slug",
                schema: "catalog",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_sort_id",
                schema: "catalog",
                table: "categories",
                columns: new[] { "parent_id", "sort_order", "id" });

            // include properties not supported by CreateIndex API -> create via SQL
            migrationBuilder.Sql(@"CREATE INDEX ix_categories_active_sort_id ON catalog.categories (is_active, sort_order, id);");
            migrationBuilder.Sql(@"-- Note: include columns name, slug, menu_label for ix_categories_active_sort_id not supported via EF Core; include columns in queries instead.");

            // catalog.brands
            migrationBuilder.CreateTable(
                name: "brands",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    long_description = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    website_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brands", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_brands_slug",
                schema: "catalog",
                table: "brands",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE INDEX ix_brands_active_sort_id ON catalog.brands (is_active, sort_order, id);");
            migrationBuilder.Sql(@"CREATE INDEX ix_brands_featured ON catalog.brands (is_featured) WHERE is_active = true;");

            // catalog.collections
            migrationBuilder.CreateTable(
                name: "collections",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    collection_type = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    long_description = table.Column<string>(type: "text", nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    badge_text = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    start_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collections", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_collections_slug",
                schema: "catalog",
                table: "collections",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE INDEX ix_collections_active_time_sort ON catalog.collections (is_active, start_at_utc, end_at_utc, sort_order);");
            migrationBuilder.Sql(@"CREATE INDEX ix_collections_featured ON catalog.collections (is_featured) WHERE is_active = true;");

            // catalog.size_guides
            migrationBuilder.CreateTable(
                name: "size_guides",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    brand_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_size_guides", x => x.id);
                    table.ForeignKey(
                        name: "fk_size_guides_brand",
                        column: x => x.brand_id,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_size_guides_slug",
                schema: "catalog",
                table: "size_guides",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ux_size_guides_brand_default ON catalog.size_guides (brand_id) WHERE is_default = true;");

            // catalog.size_guide_rows
            migrationBuilder.CreateTable(
                name: "size_guide_rows",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    size_guide_id = table.Column<long>(type: "bigint", nullable: false),
                    eu_size = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    foot_length_min_mm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    foot_length_max_mm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    note = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_size_guide_rows", x => x.id);
                    table.ForeignKey(
                        name: "fk_size_guide_rows_size_guide",
                        column: x => x.size_guide_id,
                        principalSchema: "catalog",
                        principalTable: "size_guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_size_guide_rows_eu_size",
                schema: "catalog",
                table: "size_guide_rows",
                columns: new[] { "size_guide_id", "eu_size" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_size_guide_rows_size_sort_id",
                schema: "catalog",
                table: "size_guide_rows",
                columns: new[] { "size_guide_id", "sort_order", "id" });

            // catalog.products
            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    brand_id = table.Column<long>(type: "bigint", nullable: false),
                    primary_category_id = table.Column<long>(type: "bigint", nullable: false),
                    size_guide_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    short_description = table.Column<string>(type: "text", nullable: false),
                    long_description = table.Column<string>(type: "text", nullable: true),
                    primary_color_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    style_tag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    occasion_tag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    season_tag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_purchasable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_new = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_bestseller = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_rank = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    search_keywords = table.Column<string>(type: "text", nullable: true),
                    search_synonyms = table.Column<string[]>(type: "text[]", nullable: true),
                    search_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_brand",
                        column: x => x.brand_id,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_primary_category",
                        column: x => x.primary_category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_size_guide",
                        column: x => x.size_guide_id,
                        principalSchema: "catalog",
                        principalTable: "size_guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ux_products_slug",
                schema: "catalog",
                table: "products",
                column: "slug",
                unique: true);

            // Add generated tsvector column and GIN index via SQL
            migrationBuilder.Sql(@"
ALTER TABLE catalog.products
ADD COLUMN IF NOT EXISTS search_vector tsvector
GENERATED ALWAYS AS (
  to_tsvector('simple', coalesce(name,'') || ' ' || coalesce(short_description,'' ) || ' ' || coalesce(search_keywords,''))
) STORED;
");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_products_search_vector_gin ON catalog.products USING GIN (search_vector);");

            // Partial/live indices for products
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_products_live_primary_category ON catalog.products (primary_category_id, sort_rank DESC, id DESC) WHERE status = 1 AND is_visible = true AND is_purchasable = true;");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_products_live_brand ON catalog.products (brand_id, sort_rank DESC, id DESC) WHERE status = 1 AND is_visible = true AND is_purchasable = true;");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_products_new ON catalog.products (published_at_utc DESC, id DESC) WHERE is_new = true AND status = 1 AND is_visible = true AND is_purchasable = true;");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_products_bestseller ON catalog.products (sort_rank DESC, id DESC) WHERE is_bestseller = true AND status = 1 AND is_visible = true AND is_purchasable = true;");

            // catalog.product_category_map
            migrationBuilder.CreateTable(
                name: "product_category_map",
                schema: "catalog",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_category_map", x => new { x.product_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_pcm_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pcm_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_category_map_category_sort_product",
                schema: "catalog",
                table: "product_category_map",
                columns: new[] { "category_id", "sort_order", "product_id" });

            // catalog.product_collection_map
            migrationBuilder.CreateTable(
                name: "product_collection_map",
                schema: "catalog",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    collection_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    pinned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    merchandising_score = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_collection_map", x => new { x.product_id, x.collection_id });
                    table.ForeignKey(
                        name: "fk_pcm_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pcm_collection",
                        column: x => x.collection_id,
                        principalSchema: "catalog",
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_collection_map_collection_pinned_sort_product",
                schema: "catalog",
                table: "product_collection_map",
                columns: new[] { "collection_id", "pinned", "sort_order", "product_id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_collection_map_product",
                schema: "catalog",
                table: "product_collection_map",
                column: "product_id");

            // catalog.product_variants
            migrationBuilder.CreateTable(
                name: "product_variants",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    size_eu = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    color_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    color_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    old_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character(3)", nullable: false, defaultValue: "RSD"),
                    stock_status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    total_stock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    low_stock_threshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_variants_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_product_variants_sku",
                schema: "catalog",
                table: "product_variants",
                column: "sku",
                unique: true);

            // include columns supported via raw SQL if needed; partial in-stock index
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_product_variants_in_stock ON catalog.product_variants (product_id, size_eu) WHERE is_active = true AND is_visible = true AND total_stock > 0;");

            // catalog.product_media
            migrationBuilder.CreateTable(
                name: "product_media",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    variant_id = table.Column<long>(type: "bigint", nullable: true),
                    url = table.Column<string>(type: "text", nullable: false),
                    mobile_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    alt_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    media_type = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    media_role = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_media_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_media_variant",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_media_product_sort_id",
                schema: "catalog",
                table: "product_media",
                columns: new[] { "product_id", "sort_order", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_media_role_active_sort",
                schema: "catalog",
                table: "product_media",
                columns: new[] { "product_id", "media_role", "is_active", "sort_order" });

            // partial unique primary
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ux_product_media_primary ON catalog.product_media (product_id) WHERE is_primary = true AND is_active = true;");

            // catalog.product_related_products
            migrationBuilder.CreateTable(
                name: "product_related_products",
                schema: "catalog",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    related_product_id = table.Column<long>(type: "bigint", nullable: false),
                    relation_type = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_related_products", x => new { x.product_id, x.related_product_id, x.relation_type });
                    table.ForeignKey(
                        name: "fk_prp_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_prp_related",
                        column: x => x.related_product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_related_products_product_relation_sort_related",
                schema: "catalog",
                table: "product_related_products",
                columns: new[] { "product_id", "relation_type", "sort_order", "related_product_id" });

            // inventory.stores
            migrationBuilder.CreateTable(
                name: "stores",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mall_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    working_hours_text = table.Column<string>(type: "text", nullable: true),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    directions_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    seo = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stores", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_stores_slug",
                schema: "inventory",
                table: "stores",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE INDEX ix_stores_city_active_sort_id ON inventory.stores (city, is_active, sort_order, id);");
            migrationBuilder.Sql(@"CREATE INDEX ix_stores_active_sort_id ON inventory.stores (is_active, sort_order, id);");

            // inventory.store_inventory
            migrationBuilder.CreateTable(
                name: "store_inventory",
                schema: "inventory",
                columns: table => new
                {
                    store_id = table.Column<long>(type: "bigint", nullable: false),
                    variant_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity_on_hand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store_inventory", x => new { x.store_id, x.variant_id });
                    table.ForeignKey(
                        name: "fk_store_inventory_store",
                        column: x => x.store_id,
                        principalSchema: "inventory",
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_store_inventory_variant",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "product_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_store_inventory_variant_store",
                schema: "inventory",
                table: "store_inventory",
                columns: new[] { "variant_id", "store_id" });

            migrationBuilder.Sql(@"CREATE INDEX ix_store_inventory_available ON inventory.store_inventory (variant_id, store_id) WHERE quantity_on_hand > 0;");

            // pricing.promotions
            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    discount_type = table.Column<short>(type: "smallint", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    applies_to_sale_price = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    badge_text = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    priority = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotions", x => x.id);
                });

            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ux_promotions_code_notnull ON pricing.promotions (code) WHERE code IS NOT NULL;");
            migrationBuilder.Sql(@"CREATE INDEX ix_promotions_active_time_priority ON pricing.promotions (is_active, starts_at_utc, ends_at_utc, priority);");

            // promotion join tables
            migrationBuilder.CreateTable(
                name: "promotion_products",
                schema: "pricing",
                columns: table => new
                {
                    promotion_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_products", x => new { x.promotion_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_pp_promotion",
                        column: x => x.promotion_id,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pp_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_products_product",
                schema: "pricing",
                table: "promotion_products",
                column: "product_id");

            migrationBuilder.CreateTable(
                name: "promotion_categories",
                schema: "pricing",
                columns: table => new
                {
                    promotion_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_categories", x => new { x.promotion_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_pc_promotion",
                        column: x => x.promotion_id,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pc_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_categories_category",
                schema: "pricing",
                table: "promotion_categories",
                column: "category_id");

            migrationBuilder.CreateTable(
                name: "promotion_brands",
                schema: "pricing",
                columns: table => new
                {
                    promotion_id = table.Column<long>(type: "bigint", nullable: false),
                    brand_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_brands", x => new { x.promotion_id, x.brand_id });
                    table.ForeignKey(
                        name: "fk_pb_promotion",
                        column: x => x.promotion_id,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pb_brand",
                        column: x => x.brand_id,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_brands_brand",
                schema: "pricing",
                table: "promotion_brands",
                column: "brand_id");

            migrationBuilder.CreateTable(
                name: "promotion_collections",
                schema: "pricing",
                columns: table => new
                {
                    promotion_id = table.Column<long>(type: "bigint", nullable: false),
                    collection_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_collections", x => new { x.promotion_id, x.collection_id });
                    table.ForeignKey(
                        name: "fk_pcoll_promotion",
                        column: x => x.promotion_id,
                        principalSchema: "pricing",
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pcoll_collection",
                        column: x => x.collection_id,
                        principalSchema: "catalog",
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotion_collections_collection",
                schema: "pricing",
                table: "promotion_collections",
                column: "collection_id");

            // content.site_settings
            migrationBuilder.CreateTable(
                name: "site_settings",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    default_seo_title_suffix = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    default_og_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    support_email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    support_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    social_links = table.Column<string>(type: "jsonb", nullable: true),
                    contact_info = table.Column<string>(type: "jsonb", nullable: true),
                    analytics_settings = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_settings", x => x.id);
                });

            // content.navigation_menus
            migrationBuilder.CreateTable(
                name: "navigation_menus",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    location = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_navigation_menus", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_navigation_menus_location",
                schema: "content",
                table: "navigation_menus",
                column: "location",
                unique: true);

            // content.navigation_menu_items
            migrationBuilder.CreateTable(
                name: "navigation_menu_items",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_id = table.Column<long>(type: "bigint", nullable: false),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    badge = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    opens_in_new_tab = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_navigation_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_nmi_menu",
                        column: x => x.menu_id,
                        principalSchema: "content",
                        principalTable: "navigation_menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_nmi_parent",
                        column: x => x.parent_id,
                        principalSchema: "content",
                        principalTable: "navigation_menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_navigation_menu_items_menu_parent_sort_id",
                schema: "content",
                table: "navigation_menu_items",
                columns: new[] { "menu_id", "parent_id", "sort_order", "id" });

            // content.home_pages
            migrationBuilder.CreateTable(
                name: "home_pages",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "/"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    modules = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_home_pages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_home_pages_slug",
                schema: "content",
                table: "home_pages",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ux_home_pages_single_published ON content.home_pages (is_published) WHERE is_published = true;");

            // content.category_page_contents
            migrationBuilder.CreateTable(
                name: "category_page_contents",
                schema: "content",
                columns: table => new
                {
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hero_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    hero_subtitle = table.Column<string>(type: "text", nullable: true),
                    intro_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: true),
                    seo_text = table.Column<string>(type: "text", nullable: true),
                    hero_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faq = table.Column<string>(type: "jsonb", nullable: true),
                    featured_links = table.Column<string>(type: "jsonb", nullable: true),
                    merch_blocks = table.Column<string>(type: "jsonb", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_page_contents", x => x.category_id);
                    table.ForeignKey(
                        name: "fk_cpc_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // content.brand_page_contents
            migrationBuilder.CreateTable(
                name: "brand_page_contents",
                schema: "content",
                columns: table => new
                {
                    brand_id = table.Column<long>(type: "bigint", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hero_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    hero_subtitle = table.Column<string>(type: "text", nullable: true),
                    intro_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: true),
                    seo_text = table.Column<string>(type: "text", nullable: true),
                    hero_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faq = table.Column<string>(type: "jsonb", nullable: true),
                    featured_links = table.Column<string>(type: "jsonb", nullable: true),
                    merch_blocks = table.Column<string>(type: "jsonb", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brand_page_contents", x => x.brand_id);
                    table.ForeignKey(
                        name: "fk_bpc_brand",
                        column: x => x.brand_id,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // content.collection_page_contents
            migrationBuilder.CreateTable(
                name: "collection_page_contents",
                schema: "content",
                columns: table => new
                {
                    collection_id = table.Column<long>(type: "bigint", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hero_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    hero_subtitle = table.Column<string>(type: "text", nullable: true),
                    intro_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: true),
                    seo_text = table.Column<string>(type: "text", nullable: true),
                    hero_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faq = table.Column<string>(type: "jsonb", nullable: true),
                    featured_links = table.Column<string>(type: "jsonb", nullable: true),
                    merch_blocks = table.Column<string>(type: "jsonb", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_page_contents", x => x.collection_id);
                    table.ForeignKey(
                        name: "fk_cpc_collection",
                        column: x => x.collection_id,
                        principalSchema: "catalog",
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // content.store_page_contents
            migrationBuilder.CreateTable(
                name: "store_page_contents",
                schema: "content",
                columns: table => new
                {
                    store_id = table.Column<long>(type: "bigint", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hero_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    hero_subtitle = table.Column<string>(type: "text", nullable: true),
                    intro_title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: true),
                    seo_text = table.Column<string>(type: "text", nullable: true),
                    hero_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faq = table.Column<string>(type: "jsonb", nullable: true),
                    featured_links = table.Column<string>(type: "jsonb", nullable: true),
                    merch_blocks = table.Column<string>(type: "jsonb", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store_page_contents", x => x.store_id);
                    table.ForeignKey(
                        name: "fk_spc_store",
                        column: x => x.store_id,
                        principalSchema: "inventory",
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // content.sale_pages
            migrationBuilder.CreateTable(
                name: "sale_pages",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    subtitle = table.Column<string>(type: "text", nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: true),
                    seo_text = table.Column<string>(type: "text", nullable: true),
                    hero_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    faq = table.Column<string>(type: "jsonb", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_pages", x => x.id);
                    table.ForeignKey(
                        name: "fk_sale_pages_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_sale_pages_slug",
                schema: "content",
                table: "sale_pages",
                column: "slug",
                unique: true);

            // content.editorial_articles
            migrationBuilder.CreateTable(
                name: "editorial_articles",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    slug = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    excerpt = table.Column<string>(type: "text", nullable: false),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    body = table.Column<string>(type: "jsonb", nullable: false),
                    topic = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    author_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    reading_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_editorial_articles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_editorial_articles_slug",
                schema: "content",
                table: "editorial_articles",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_editorial_articles_published ON content.editorial_articles (published_at_utc DESC, id DESC) WHERE status = 1;");

            // editorial join tables
            migrationBuilder.CreateTable(
                name: "editorial_article_products",
                schema: "content",
                columns: table => new
                {
                    editorial_article_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_editorial_article_products", x => new { x.editorial_article_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_eap_article",
                        column: x => x.editorial_article_id,
                        principalSchema: "content",
                        principalTable: "editorial_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eap_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_editorial_article_products_product",
                schema: "content",
                table: "editorial_article_products",
                column: "product_id");

            migrationBuilder.CreateTable(
                name: "editorial_article_categories",
                schema: "content",
                columns: table => new
                {
                    editorial_article_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_editorial_article_categories", x => new { x.editorial_article_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_eac_article",
                        column: x => x.editorial_article_id,
                        principalSchema: "content",
                        principalTable: "editorial_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eac_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_editorial_article_categories_category",
                schema: "content",
                table: "editorial_article_categories",
                column: "category_id");

            migrationBuilder.CreateTable(
                name: "editorial_article_brands",
                schema: "content",
                columns: table => new
                {
                    editorial_article_id = table.Column<long>(type: "bigint", nullable: false),
                    brand_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_editorial_article_brands", x => new { x.editorial_article_id, x.brand_id });
                    table.ForeignKey(
                        name: "fk_eab_article",
                        column: x => x.editorial_article_id,
                        principalSchema: "content",
                        principalTable: "editorial_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eab_brand",
                        column: x => x.brand_id,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_editorial_article_brands_brand",
                schema: "content",
                table: "editorial_article_brands",
                column: "brand_id");

            migrationBuilder.CreateTable(
                name: "editorial_article_collections",
                schema: "content",
                columns: table => new
                {
                    editorial_article_id = table.Column<long>(type: "bigint", nullable: false),
                    collection_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_editorial_article_collections", x => new { x.editorial_article_id, x.collection_id });
                    table.ForeignKey(
                        name: "fk_eacoll_article",
                        column: x => x.editorial_article_id,
                        principalSchema: "content",
                        principalTable: "editorial_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eacoll_collection",
                        column: x => x.collection_id,
                        principalSchema: "catalog",
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_editorial_article_collections_collection",
                schema: "content",
                table: "editorial_article_collections",
                column: "collection_id");

            // content.trust_pages
            migrationBuilder.CreateTable(
                name: "trust_pages",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_kind = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    body = table.Column<string>(type: "jsonb", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    seo = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trust_pages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_trust_pages_slug",
                schema: "content",
                table: "trust_pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_trust_pages_page_kind",
                schema: "content",
                table: "trust_pages",
                column: "page_kind",
                unique: true);

            // shared.slug_redirects
            migrationBuilder.CreateTable(
                name: "slug_redirects",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type = table.Column<short>(type: "smallint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    old_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    new_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status_code = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)301),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_slug_redirects", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_slug_redirects_old_path",
                schema: "shared",
                table: "slug_redirects",
                column: "old_path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_slug_redirects_new_path",
                schema: "shared",
                table: "slug_redirects",
                column: "new_path");

            // Seed categories
            var now = DateTimeOffset.UtcNow;

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "categories",
                columns: new[] { "id", "parent_id", "name", "slug", "menu_label", "short_description", "image_url", "sort_order", "depth", "is_active", "type", "created_at_utc", "updated_at_utc" },
                values: new object[,]
                {
                    { 1L, null, "Cipele", "cipele", null, null, null, 0, (short)0, true, (short)1, now, now },
                    { 2L, null, "Patike", "patike", null, null, null, 0, (short)0, true, (short)1, now, now },
                    { 3L, null, "Čizme", "cizme", null, null, null, 0, (short)0, true, (short)1, now, now },
                    { 4L, null, "Sandale", "sandale", null, null, null, 0, (short)0, true, (short)1, now, now },
                    { 5L, null, "Papuče", "papuce", null, null, null, 0, (short)0, true, (short)1, now, now },
                    { 101L, 1L, "Salonke", "salonke", null, null, null, 0, (short)1, true, (short)2, now, now },
                    { 102L, 1L, "Baletanke", "baletanke", null, null, null, 0, (short)1, true, (short)2, now, now },
                    { 103L, 1L, "Mokasine", "mokasine", null, null, null, 0, (short)1, true, (short)2, now, now },
                    { 104L, 3L, "Gležnjače", "gleznjace", null, null, null, 0, (short)1, true, (short)2, now, now },
                    { 105L, 2L, "Lifestyle", "lifestyle", null, null, null, 0, (short)1, true, (short)2, now, now }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(schema: "catalog", table: "categories", keyColumn: "id", keyValues: new object[] { 1L,2L,3L,4L,5L,101L,102L,103L,104L,105L });

            migrationBuilder.DropTable(name: "product_collection_map", schema: "catalog");
            migrationBuilder.DropTable(name: "product_category_map", schema: "catalog");
            migrationBuilder.DropTable(name: "product_related_products", schema: "catalog");
            migrationBuilder.DropTable(name: "product_media", schema: "catalog");
            migrationBuilder.DropTable(name: "product_variants", schema: "catalog");
            migrationBuilder.DropTable(name: "products", schema: "catalog");
            migrationBuilder.DropTable(name: "size_guide_rows", schema: "catalog");
            migrationBuilder.DropTable(name: "size_guides", schema: "catalog");
            migrationBuilder.DropTable(name: "collections", schema: "catalog");
            migrationBuilder.DropTable(name: "brands", schema: "catalog");
            migrationBuilder.DropTable(name: "categories", schema: "catalog");

            migrationBuilder.DropTable(name: "store_inventory", schema: "inventory");
            migrationBuilder.DropTable(name: "stores", schema: "inventory");

            migrationBuilder.DropTable(name: "promotion_products", schema: "pricing");
            migrationBuilder.DropTable(name: "promotion_categories", schema: "pricing");
            migrationBuilder.DropTable(name: "promotion_brands", schema: "pricing");
            migrationBuilder.DropTable(name: "promotion_collections", schema: "pricing");
            migrationBuilder.DropTable(name: "promotions", schema: "pricing");

            migrationBuilder.DropTable(name: "site_settings", schema: "content");
            migrationBuilder.DropTable(name: "navigation_menu_items", schema: "content");
            migrationBuilder.DropTable(name: "navigation_menus", schema: "content");
            migrationBuilder.DropTable(name: "home_pages", schema: "content");
            migrationBuilder.DropTable(name: "category_page_contents", schema: "content");
            migrationBuilder.DropTable(name: "brand_page_contents", schema: "content");
            migrationBuilder.DropTable(name: "collection_page_contents", schema: "content");
            migrationBuilder.DropTable(name: "store_page_contents", schema: "content");
            migrationBuilder.DropTable(name: "sale_pages", schema: "content");
            migrationBuilder.DropTable(name: "editorial_article_products", schema: "content");
            migrationBuilder.DropTable(name: "editorial_article_categories", schema: "content");
            migrationBuilder.DropTable(name: "editorial_article_brands", schema: "content");
            migrationBuilder.DropTable(name: "editorial_article_collections", schema: "content");
            migrationBuilder.DropTable(name: "editorial_articles", schema: "content");
            migrationBuilder.DropTable(name: "trust_pages", schema: "content");

            migrationBuilder.DropTable(name: "slug_redirects", schema: "shared");

            // Drop custom indexes created by SQL
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_products_search_vector_gin;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_products_live_primary_category;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_products_live_brand;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_products_new;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_products_bestseller;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ux_product_media_primary;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_product_variants_in_stock;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_store_inventory_available;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ux_promotions_code_notnull;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_editorial_articles_published;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ux_home_pages_single_published;");

            // Drop schemas
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS catalog CASCADE;");
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS inventory CASCADE;");
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS pricing CASCADE;");
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS content CASCADE;");
            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS shared CASCADE;");
        }
    }
}
