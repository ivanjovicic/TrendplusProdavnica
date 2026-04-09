import { apiClient } from './api-client';
import type { HomePageDto, ProductListingPageDto, ProductdetailDto, BrandPageDto, CollectionPageDto, EditorialArticleDto, StorePageDto } from '@/lib/types';

// Pages
export async function getHomePage() {
  return apiClient.get<HomePageDto>('/pages/home');
}

// Listings
interface ListingParams {
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

// Products
export async function getProductDetail(slug: string) {
  return apiClient.get<ProductdetailDto>(`/products/${slug}`);
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
