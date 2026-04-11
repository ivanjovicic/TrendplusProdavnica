import type { MetadataRoute } from 'next';
import { API_BASE_URL } from '@/lib/config/env';
import { buildLanguageAlternates } from './metadata';
import { SEO_SITE_URL } from './site';

interface ProductSitemapItem {
  slug: string;
  isVisible: boolean;
  isPurchasable: boolean;
  isIndexable: boolean;
  updatedAtUtc: string;
}

interface SeoItem {
  slug: string;
  isActive: boolean;
  isIndexable: boolean;
  updatedAtUtc: string;
}

function toAbsolute(path: string): string {
  return new URL(path, `${SEO_SITE_URL}/`).toString();
}

function createSitemapEntry(
  path: string,
  options: Omit<MetadataRoute.Sitemap[number], 'url' | 'alternates'>,
): MetadataRoute.Sitemap[number] {
  return {
    url: toAbsolute(path),
    alternates: {
      languages: buildLanguageAlternates(path),
    },
    ...options,
  };
}

async function fetchSeoJson<T>(endpoint: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    next: { revalidate: 3600 },
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch SEO data from ${endpoint}`);
  }

  return response.json() as Promise<T>;
}

function asDate(value?: string): Date | undefined {
  if (!value) {
    return undefined;
  }

  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? undefined : parsed;
}

export async function buildStorefrontSitemap(): Promise<MetadataRoute.Sitemap> {
  const [categories, brands, collections, products, stores, editorial] = await Promise.all([
    fetchSeoJson<SeoItem[]>('/api/seo/categories').catch(() => []),
    fetchSeoJson<SeoItem[]>('/api/seo/brands').catch(() => []),
    fetchSeoJson<SeoItem[]>('/api/seo/collections').catch(() => []),
    fetchSeoJson<ProductSitemapItem[]>('/api/seo/products').catch(() => []),
    fetchSeoJson<SeoItem[]>('/api/seo/stores').catch(() => []),
    fetchSeoJson<SeoItem[]>('/api/seo/editorial').catch(() => []),
  ]);

  const now = new Date();

  const staticEntries: MetadataRoute.Sitemap = [
    createSitemapEntry('/', {
      lastModified: now,
      changeFrequency: 'daily',
      priority: 1,
    }),
    createSitemapEntry('/akcija', {
      lastModified: now,
      changeFrequency: 'daily',
      priority: 0.8,
    }),
    createSitemapEntry('/editorial', {
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.7,
    }),
    createSitemapEntry('/prodavnice', {
      lastModified: now,
      changeFrequency: 'weekly',
      priority: 0.7,
    }),
  ];

  const categoryEntries = categories
    .filter((category) => category.isActive && category.isIndexable)
    .map((category) =>
      createSitemapEntry(`/kategorije/${category.slug}`, {
        lastModified: asDate(category.updatedAtUtc) || now,
        changeFrequency: 'daily',
        priority: 0.8,
      }),
    );

  const brandEntries = brands
    .filter((brand) => brand.isActive && brand.isIndexable)
    .map((brand) =>
      createSitemapEntry(`/brendovi/${brand.slug}`, {
        lastModified: asDate(brand.updatedAtUtc) || now,
        changeFrequency: 'weekly',
        priority: 0.7,
      }),
    );

  const collectionEntries = collections
    .filter((collection) => collection.isActive && collection.isIndexable)
    .map((collection) =>
      createSitemapEntry(`/kolekcije/${collection.slug}`, {
        lastModified: asDate(collection.updatedAtUtc) || now,
        changeFrequency: 'weekly',
        priority: 0.7,
      }),
    );

  const productEntries = products
    .filter((product) => product.isVisible && product.isPurchasable && product.isIndexable)
    .map((product) =>
      createSitemapEntry(`/proizvod/${product.slug}`, {
        lastModified: asDate(product.updatedAtUtc) || now,
        changeFrequency: 'weekly',
        priority: 0.9,
      }),
    );

  const storeEntries = stores
    .filter((store) => store.isActive && store.isIndexable)
    .map((store) =>
      createSitemapEntry(`/prodavnice/${store.slug}`, {
        lastModified: asDate(store.updatedAtUtc) || now,
        changeFrequency: 'weekly',
        priority: 0.6,
      }),
    );

  const editorialEntries = editorial
    .filter((article) => article.isActive && article.isIndexable)
    .map((article) =>
      createSitemapEntry(`/editorial/${article.slug}`, {
        lastModified: asDate(article.updatedAtUtc) || now,
        changeFrequency: 'monthly',
        priority: 0.6,
      }),
    );

  return [
    ...staticEntries,
    ...categoryEntries,
    ...brandEntries,
    ...collectionEntries,
    ...productEntries,
    ...storeEntries,
    ...editorialEntries,
  ];
}
