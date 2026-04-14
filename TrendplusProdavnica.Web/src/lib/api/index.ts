import { apiClient } from './api-client';
import type {
  HomePageDto,
  ProductListingPageDto,
  ProductdetailDto,
  BrandPageDto,
  CollectionPageDto,
  EditorialArticleDto,
  ProductAutocompleteResultDto,
  SearchResponseDto,
  StorePageDto,
} from '@/lib/types';

export * from './cart';
export * from './wishlist';
export * from './checkout';
export * from './analytics';

export { apiClient as getApiClient };

export async function getHomePage() {
  return apiClient.get<HomePageDto>('/pages/home');
}

// Listings
interface ListingParams {
  [key: string]: string | number | boolean | undefined;
  page?: number;
  pageSize?: number;
  sort?: string;
  sizes?: string;
  colors?: string;
  brands?: string;
  priceFrom?: number;
  priceTo?: number;
  isOnSale?: boolean;
  isNew?: boolean;
  inStockOnly?: boolean;
}

export async function getCategoryListing(slug: string, params?: ListingParams) {
  return apiClient.get<ProductListingPageDto>(`/listings/category/${slug}`, { searchParams: params });
}

export async function getBrandListing(slug: string, params?: ListingParams) {
  return apiClient.get<ProductListingPageDto>(`/listings/brand/${slug}`, { searchParams: params });
}

export async function getCollectionListing(slug: string, params?: ListingParams) {
  return apiClient.get<ProductListingPageDto>(`/listings/collection/${slug}`, { searchParams: params });
}

export async function getSaleListing(params?: ListingParams) {
  return apiClient.get<ProductListingPageDto>(`/listings/sale`, { searchParams: params });
}

export async function getSaleCategoryListing(categorySlug: string, params?: ListingParams) {
  return apiClient.get<ProductListingPageDto>(`/listings/sale/${categorySlug}`, { searchParams: params });
}

interface SearchParams {
  [key: string]: string | number | boolean | string[] | number[] | undefined;
  q?: string;
  page?: number;
  pageSize?: number;
  brands?: string[];
  colors?: string[];
  sizes?: number[];
  minPrice?: number;
  maxPrice?: number;
  availability?: string[];
  isOnSale?: boolean;
  isNew?: boolean;
  sort?: string;
}

export async function searchProducts(params?: SearchParams) {
  return apiClient.get<SearchResponseDto>('/search', { searchParams: params });
}

export async function autocompleteProducts(
  q: string,
  limit = 6,
  options?: RequestInit,
) {
  return apiClient.get<ProductAutocompleteResultDto>('/search/autocomplete', {
    ...options,
    searchParams: { q, limit },
  });
}

// Products
export async function getProductDetail(slug: string) {
  return apiClient.get<ProductdetailDto>(`/catalog/product/${slug}`);
}

// Brands
export async function getBrandPage(slug: string) {
  return apiClient.get<BrandPageDto>(`/brands/${slug}`);
}

// Collections
export async function getCollectionPage(slug: string) {
  return apiClient.get<CollectionPageDto>(`/collections/${slug}`);
}

// Editorial
export async function getEditorialList() {
  return apiClient.get<EditorialArticleDto[]>('/editorial');
}

export async function getEditorialArticle(slug: string) {
  return apiClient.get<EditorialArticleDto>(`/editorial/${slug}`);
}

// Stores
interface StoresParams {
  [key: string]: string | number | boolean | undefined;
  city?: string;
  page?: number;
  pageSize?: number;
}

export async function getStores(params?: StoresParams) {
  return apiClient.get<{ items: StorePageDto[]; totalCount: number }>('/stores', { searchParams: params });
}

export async function getStore(slug: string) {
  return apiClient.get<StorePageDto>(`/stores/${slug}`);
}
