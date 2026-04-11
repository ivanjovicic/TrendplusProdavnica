\pset pager off
\timing on

\echo 'Resolving benchmark fixtures from current database...'

SELECT "Slug" AS category_slug
FROM catalog.categories
WHERE "IsActive" = true
ORDER BY "SortOrder", "Id"
LIMIT 1
\gset

SELECT "Slug" AS brand_slug
FROM catalog.brands
WHERE "IsActive" = true
ORDER BY "SortOrder", "Id"
LIMIT 1
\gset

SELECT "Slug" AS collection_slug
FROM catalog.collections
WHERE "IsActive" = true
ORDER BY "SortOrder", "Id"
LIMIT 1
\gset

SELECT "Slug" AS product_slug
FROM catalog.products
WHERE "Status" = 1
  AND "IsVisible" = true
  AND "IsPurchasable" = true
ORDER BY "PublishedAtUtc" DESC NULLS LAST, "Id" DESC
LIMIT 1
\gset

\echo ''
\echo 'Using fixtures:'
\echo '  category_slug   = :category_slug'
\echo '  brand_slug      = :brand_slug'
\echo '  collection_slug = :collection_slug'
\echo '  product_slug    = :product_slug'
\echo ''

\echo '01. Category lookup by slug'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT "Id"
FROM catalog.categories
WHERE "Slug" = :'category_slug'
  AND "IsActive" = true;

\echo ''
\echo '02. Brand lookup by slug'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT "Id"
FROM catalog.brands
WHERE "Slug" = :'brand_slug'
  AND "IsActive" = true;

\echo ''
\echo '03. Collection lookup by slug'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT "Id"
FROM catalog.collections
WHERE "Slug" = :'collection_slug'
  AND "IsActive" = true;

\echo ''
\echo '04. Home page root query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT "Title", "Seo_SeoTitle", "Seo_SeoDescription", "Modules"
FROM content.home_pages
WHERE "IsPublished" = true
ORDER BY "PublishedAtUtc" DESC NULLS LAST, "Id" DESC
LIMIT 1;

\echo ''
\echo '05. Home page new arrivals rail'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT p."Id", p."Slug", p."Name", p."PublishedAtUtc", p."SortRank"
FROM catalog.products p
WHERE p."Status" = 1
  AND p."IsVisible" = true
  AND p."IsPurchasable" = true
  AND p."IsNew" = true
ORDER BY p."PublishedAtUtc" DESC NULLS LAST, p."SortRank" DESC, p."Id" DESC
LIMIT 12;

\echo ''
\echo '06. PLP total count for category scope'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
WITH category_scope AS (
    SELECT "Id"
    FROM catalog.categories
    WHERE "Slug" = :'category_slug'
      AND "IsActive" = true
)
SELECT count(*)
FROM catalog.products p
CROSS JOIN category_scope c
WHERE p."Status" = 1
  AND p."IsVisible" = true
  AND p."IsPurchasable" = true
  AND EXISTS (
      SELECT 1
      FROM catalog.product_variants v
      WHERE v."ProductId" = p."Id"
        AND v."IsActive" = true
        AND v."IsVisible" = true
  )
  AND (
      p."PrimaryCategoryId" = c."Id"
      OR EXISTS (
          SELECT 1
          FROM catalog.product_category_map pcm
          WHERE pcm."ProductId" = p."Id"
            AND pcm."CategoryId" = c."Id"
      )
  );

\echo ''
\echo '07. PLP page query with price, media and size aggregates'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
WITH category_scope AS (
    SELECT "Id"
    FROM catalog.categories
    WHERE "Slug" = :'category_slug'
      AND "IsActive" = true
),
page_products AS (
    SELECT p."Id", p."Slug", p."Name", p."BrandId", p."IsNew", p."PrimaryColorName", p."SortRank", p."PublishedAtUtc"
    FROM catalog.products p
    CROSS JOIN category_scope c
    WHERE p."Status" = 1
      AND p."IsVisible" = true
      AND p."IsPurchasable" = true
      AND EXISTS (
          SELECT 1
          FROM catalog.product_variants v
          WHERE v."ProductId" = p."Id"
            AND v."IsActive" = true
            AND v."IsVisible" = true
      )
      AND (
          p."PrimaryCategoryId" = c."Id"
          OR EXISTS (
              SELECT 1
              FROM catalog.product_category_map pcm
              WHERE pcm."ProductId" = p."Id"
                AND pcm."CategoryId" = c."Id"
          )
      )
    ORDER BY p."SortRank" DESC, p."PublishedAtUtc" DESC NULLS LAST, p."Id" DESC
    LIMIT 24 OFFSET 0
)
SELECT
    pp."Id",
    pp."Slug",
    pp."Name",
    b."Name" AS "BrandName",
    price_stats."MinPrice",
    sale_stats."MinOldPrice",
    primary_media."PrimaryImageUrl",
    secondary_media."SecondaryImageUrl",
    size_stats."AvailableSizesCount"
FROM page_products pp
JOIN catalog.brands b
    ON b."Id" = pp."BrandId"
LEFT JOIN LATERAL (
    SELECT min(v."Price") AS "MinPrice"
    FROM catalog.product_variants v
    WHERE v."ProductId" = pp."Id"
      AND v."IsActive" = true
      AND v."IsVisible" = true
) price_stats ON true
LEFT JOIN LATERAL (
    SELECT min(v."OldPrice") AS "MinOldPrice"
    FROM catalog.product_variants v
    WHERE v."ProductId" = pp."Id"
      AND v."IsActive" = true
      AND v."IsVisible" = true
      AND v."OldPrice" IS NOT NULL
      AND v."OldPrice" > v."Price"
) sale_stats ON true
LEFT JOIN LATERAL (
    SELECT m."Url" AS "PrimaryImageUrl"
    FROM catalog.product_media m
    WHERE m."ProductId" = pp."Id"
      AND m."IsActive" = true
      AND m."IsPrimary" = true
    ORDER BY m."SortOrder", m."Id"
    LIMIT 1
) primary_media ON true
LEFT JOIN LATERAL (
    SELECT m."Url" AS "SecondaryImageUrl"
    FROM catalog.product_media m
    WHERE m."ProductId" = pp."Id"
      AND m."IsActive" = true
      AND m."IsPrimary" = false
    ORDER BY m."SortOrder", m."Id"
    LIMIT 1
) secondary_media ON true
LEFT JOIN LATERAL (
    SELECT count(DISTINCT v."SizeEu")::integer AS "AvailableSizesCount"
    FROM catalog.product_variants v
    WHERE v."ProductId" = pp."Id"
      AND v."IsActive" = true
      AND v."IsVisible" = true
      AND v."TotalStock" > 0
) size_stats ON true
ORDER BY pp."SortRank" DESC, pp."PublishedAtUtc" DESC NULLS LAST, pp."Id" DESC;

\echo ''
\echo '08. PDP root query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT
    p."Id",
    p."Slug",
    p."Name",
    p."Subtitle",
    p."ShortDescription",
    p."LongDescription",
    p."BrandId",
    b."Name" AS "BrandName",
    b."Slug" AS "BrandSlug",
    c."Id" AS "CategoryId",
    c."Name" AS "CategoryName",
    c."Slug" AS "CategorySlug",
    p."PrimaryColorName",
    p."SizeGuideId",
    p."PublishedAtUtc"
FROM catalog.products p
JOIN catalog.brands b ON b."Id" = p."BrandId"
JOIN catalog.categories c ON c."Id" = p."PrimaryCategoryId"
WHERE p."Slug" = :'product_slug'
  AND p."Status" = 1
  AND p."IsVisible" = true
  AND p."IsPurchasable" = true;

\echo ''
\echo '09. PDP variants query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT
    v."Id",
    v."ProductId",
    v."Sku",
    v."Barcode",
    v."SizeEu",
    v."Price",
    v."OldPrice",
    v."Currency",
    v."IsActive",
    v."IsVisible",
    v."TotalStock",
    v."StockStatus",
    v."LowStockThreshold"
FROM catalog.product_variants v
JOIN catalog.products p ON p."Id" = v."ProductId"
WHERE p."Slug" = :'product_slug'
ORDER BY v."SortOrder", v."Id";

\echo ''
\echo '10. PDP media query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT
    m."Id",
    m."ProductId",
    m."VariantId",
    m."Url",
    m."AltText",
    m."Title",
    m."IsPrimary",
    m."IsActive",
    m."SortOrder"
FROM catalog.product_media m
JOIN catalog.products p ON p."Id" = m."ProductId"
WHERE p."Slug" = :'product_slug'
  AND m."IsActive" = true
ORDER BY m."SortOrder", m."Id";

\echo ''
\echo '11. PDP rating summary query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT
    r."ProductId",
    r."AverageRating",
    r."ReviewCount",
    r."RatingCount"
FROM catalog.product_ratings r
JOIN catalog.products p ON p."Id" = r."ProductId"
WHERE p."Slug" = :'product_slug';

\echo ''
\echo '12. PDP published reviews query'
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, SETTINGS)
SELECT
    pr."ProductId",
    pr."AuthorName",
    pr."Title",
    pr."ReviewBody",
    pr."RatingValue",
    pr."PublishedAtUtc"
FROM catalog.product_reviews pr
JOIN catalog.products p ON p."Id" = pr."ProductId"
WHERE p."Slug" = :'product_slug'
  AND pr."Status" = 1
ORDER BY pr."PublishedAtUtc" DESC NULLS LAST, pr."Id" DESC
LIMIT 10;
