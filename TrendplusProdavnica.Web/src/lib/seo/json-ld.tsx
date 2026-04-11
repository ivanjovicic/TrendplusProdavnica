import type {
  BreadcrumbItemDto,
  ProductAggregateRatingDto,
  ProductReviewDto,
  ProductSizeOptionDto,
  ProductdetailDto,
  SeoDto,
  StorePageDto,
} from '@/lib/types';
import { absoluteUrl } from './metadata';
import {
  SEO_DEFAULT_DESCRIPTION,
  SEO_DEFAULT_LANGUAGE_TAG,
  SEO_SITE_NAME,
  SEO_SITE_URL,
} from './site';

function serializeJsonLd(data: object): string {
  return JSON.stringify(data).replace(/</g, '\\u003c');
}

function parseStructuredDataOverride(seo?: SeoDto | null): Record<string, unknown> | undefined {
  const raw = seo?.structuredDataOverrideJson;
  if (!raw) {
    return undefined;
  }

  try {
    const parsed = JSON.parse(raw);
    return parsed && typeof parsed === 'object' && !Array.isArray(parsed)
      ? (parsed as Record<string, unknown>)
      : undefined;
  } catch {
    return undefined;
  }
}

function withStructuredDataOverride<T extends Record<string, unknown>>(base: T, seo?: SeoDto | null): T {
  const override = parseStructuredDataOverride(seo);
  return override ? ({ ...base, ...override } as T) : base;
}

function getVisibleSizes(product: ProductdetailDto): ProductSizeOptionDto[] {
  return product.sizes.filter((size) => size.isActive && size.isVisible);
}

function getOfferAvailability(size: ProductSizeOptionDto): string {
  return size.totalStock > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock';
}

function buildAggregateRating(
  rating?: ProductAggregateRatingDto,
  averageRating?: number,
  reviewCount?: number,
  ratingCount?: number,
) {
  if (rating) {
    return {
      '@type': 'AggregateRating',
      ratingValue: rating.ratingValue,
      reviewCount: rating.reviewCount,
      ratingCount: rating.ratingCount || rating.reviewCount,
      bestRating: rating.bestRating || 5,
      worstRating: rating.worstRating || 1,
    };
  }

  if (typeof averageRating === 'number' && typeof reviewCount === 'number' && reviewCount > 0) {
    return {
      '@type': 'AggregateRating',
      ratingValue: averageRating,
      reviewCount,
      ratingCount: ratingCount || reviewCount,
      bestRating: 5,
      worstRating: 1,
    };
  }

  return undefined;
}

function buildReviews(reviews?: ProductReviewDto[]) {
  if (!reviews || reviews.length === 0) {
    return undefined;
  }

  return reviews
    .filter((review) => typeof review.ratingValue === 'number')
    .slice(0, 10)
    .map((review) => ({
      '@type': 'Review',
      author: {
        '@type': 'Person',
        name: review.authorName || 'Kupac',
      },
      reviewRating: {
        '@type': 'Rating',
        ratingValue: review.ratingValue,
        bestRating: 5,
        worstRating: 1,
      },
      name: review.title,
      reviewBody: review.reviewBody,
      datePublished: review.publishedAtUtc,
    }));
}

function buildProductOffers(product: ProductdetailDto) {
  const visibleSizes = getVisibleSizes(product);
  const sizesForOffers = visibleSizes.length > 0 ? visibleSizes : product.sizes.slice(0, 1);

  if (sizesForOffers.length === 0) {
    return {
      '@type': 'Offer',
      url: absoluteUrl(`/proizvod/${product.slug}`),
      priceCurrency: product.currency,
      price: product.price,
      availability: 'https://schema.org/InStock',
      itemCondition: 'https://schema.org/NewCondition',
      seller: {
        '@type': 'Organization',
        name: SEO_SITE_NAME,
      },
    };
  }

  const individualOffers = sizesForOffers.map((size) => ({
    '@type': 'Offer',
    url: absoluteUrl(`/proizvod/${product.slug}`),
    priceCurrency: size.currency || product.currency,
    price: size.price,
    availability: getOfferAvailability(size),
    itemCondition: 'https://schema.org/NewCondition',
    sku: size.sku,
    gtin13: size.barcode,
    seller: {
      '@type': 'Organization',
      name: SEO_SITE_NAME,
    },
  }));

  if (individualOffers.length === 1) {
    return individualOffers[0];
  }

  const prices = individualOffers.map((offer) => Number(offer.price));

  return {
    '@type': 'AggregateOffer',
    url: absoluteUrl(`/proizvod/${product.slug}`),
    priceCurrency: product.currency,
    lowPrice: Math.min(...prices),
    highPrice: Math.max(...prices),
    offerCount: individualOffers.length,
    availability: individualOffers.some((offer) => offer.availability.endsWith('/InStock'))
      ? 'https://schema.org/InStock'
      : 'https://schema.org/OutOfStock',
    offers: individualOffers,
  };
}

export function JsonLd({ data }: { data: object | null | undefined }) {
  if (!data) {
    return null;
  }

  return (
    <script
      type="application/ld+json"
      suppressHydrationWarning
      dangerouslySetInnerHTML={{ __html: serializeJsonLd(data) }}
    />
  );
}

export function buildOrganizationJsonLd() {
  return {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: SEO_SITE_NAME,
    url: SEO_SITE_URL,
    description: SEO_DEFAULT_DESCRIPTION,
    inLanguage: SEO_DEFAULT_LANGUAGE_TAG,
  };
}

export function buildBreadcrumbListJsonLd(items: BreadcrumbItemDto[]) {
  return {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items.map((item, index) => ({
      '@type': 'ListItem',
      position: index + 1,
      name: item.label,
      item: absoluteUrl(item.url),
    })),
  };
}

export function buildProductJsonLd(product: ProductdetailDto) {
  const imageUrls = product.media.map((media) => absoluteUrl(media.url));
  const primarySize = getVisibleSizes(product)[0] || product.sizes[0];
  const aggregateRating = buildAggregateRating(
    product.aggregateRating,
    product.averageRating,
    product.reviewCount,
    product.ratingCount,
  );
  const reviews = buildReviews(product.reviews);

  const jsonLd = {
    '@context': 'https://schema.org',
    '@type': 'Product',
    name: product.name,
    description: product.shortDescription || product.longDescription,
    url: absoluteUrl(`/proizvod/${product.slug}`),
    image: imageUrls,
    sku: product.sku || primarySize?.sku,
    mpn: product.mpn,
    gtin: product.gtin,
    gtin13: product.gtin13 || primarySize?.barcode,
    color: product.primaryColorName,
    category: product.categoryName,
    brand: {
      '@type': 'Brand',
      name: product.brandName,
    },
    offers: buildProductOffers(product),
    aggregateRating,
    review: reviews,
    inLanguage: SEO_DEFAULT_LANGUAGE_TAG,
  };

  return withStructuredDataOverride(jsonLd, product.seo);
}

export function buildStoreJsonLd(store: StorePageDto) {
  const streetAddress = store.addressLine1 || store.address;
  const image = store.coverImageUrl || store.imageUrl;
  const openingHours = store.workingHoursText || store.operatingHours;

  return withStructuredDataOverride(
    {
      '@context': 'https://schema.org',
      '@type': 'Store',
      name: store.name,
      url: absoluteUrl(`/prodavnice/${store.slug}`),
      image: image ? absoluteUrl(image) : undefined,
      telephone: store.phone,
      email: store.email,
      description: store.shortDescription || store.description,
      address: {
        '@type': 'PostalAddress',
        streetAddress,
        addressLocality: store.city,
        postalCode: store.postalCode,
        addressCountry: store.country || 'RS',
      },
      geo:
        typeof store.latitude === 'number' && typeof store.longitude === 'number'
          ? {
              '@type': 'GeoCoordinates',
              latitude: store.latitude,
              longitude: store.longitude,
            }
          : undefined,
      openingHours,
      inLanguage: SEO_DEFAULT_LANGUAGE_TAG,
    },
    store.seo,
  );
}
