// SEO & Metadata
export interface SeoDto {
  seoTitle?: string;
  seoDescription?: string;
  canonicalUrl?: string;
  alternateLanguageUrls?: Record<string, string>;
  robotsDirective?: string;
  ogTitle?: string;
  ogDescription?: string;
  ogImageUrl?: string;
  structuredDataOverrideJson?: string;
}

export interface BreadcrumbItemDto {
  label: string;
  slug?: string;
  url: string;
}

// Product Related
export interface ProductMediaDto {
  id: number;
  productId: number;
  variantId?: number;
  url: string;
  mobileUrl?: string;
  altText?: string;
  title?: string;
  mediaType: number;
  mediaRole: number;
  isPrimary: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface ProductSizeOptionDto {
  variantId?: number;
  sizeEu: number;
  sku: string;
  barcode?: string;
  price: number;
  oldPrice?: number;
  currency: string;
  isActive: boolean;
  isVisible: boolean;
  totalStock: number;
  stockStatus: number;
  lowStockThreshold: number;
}

export interface ProductCardDto {
  id: number;
  productId?: number;
  slug: string;
  name: string;
  subtitle?: string;
  shortDescription: string;
  brandId: number;
  brandName: string;
  primaryColorName?: string;
  primaryImageUrl?: string;
  secondaryImageUrl?: string;
  price: number;
  oldPrice?: number;
  currency: string;
  isNew: boolean;
  isBestseller: boolean;
  isOnSale: boolean;
  availableSizes: number;
  availableSizesCount?: number;
  isVisibleInStorefront: boolean;
}

export interface ProductAggregateRatingDto {
  ratingValue: number;
  reviewCount: number;
  ratingCount?: number;
  bestRating?: number;
  worstRating?: number;
}

export interface ProductReviewDto {
  authorName?: string;
  title?: string;
  reviewBody?: string;
  ratingValue: number;
  publishedAtUtc?: string;
}

export interface ProductdetailDto {
  id: number;
  slug: string;
  name: string;
  subtitle?: string;
  shortDescription: string;
  longDescription?: string;
  brandId: number;
  brandName: string;
  brandSlug: string;
  categoryId: number;
  categoryName: string;
  categorySlug: string;
  primaryColorName?: string;
  secondaryColorName?: string;
  sizeGuideId?: number;
  sku?: string;
  mpn?: string;
  gtin?: string;
  gtin13?: string;
  price: number;
  oldPrice?: number;
  currency: string;
  isNew: boolean;
  isBestseller: boolean;
  isOnSale: boolean;
  media: ProductMediaDto[];
  sizes: ProductSizeOptionDto[];
  aggregateRating?: ProductAggregateRatingDto;
  averageRating?: number;
  reviewCount?: number;
  ratingCount?: number;
  reviews?: ProductReviewDto[];
  deliveryInfo?: string;
  returnsInfo?: string;
  careInstructions?: string;
  seo?: SeoDto;
  relatedByBrand?: ProductCardDto[];
  relatedBySimilarity?: ProductCardDto[];
}

export interface ProductListingPageDto {
  title: string;
  slug: string;
  intro?: string;
  breadcrumbs: BreadcrumbItemDto[];
  products: ProductCardDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  sortOptions: Array<{ label: string; value: string }>;
  availableFilters: {
    brands?: string[];
    colors?: string[];
    sizes?: number[];
    priceRange?: { min?: number; max?: number };
  };
  merchBlocks?: unknown[];
  faq?: unknown;
  seo?: SeoDto;
}

export interface SearchFacetValueDto {
  value: string;
  label: string;
  count: number;
  selected: boolean;
}

export interface SearchPriceRangeFacetDto {
  min?: number;
  max?: number;
  selectedMin?: number;
  selectedMax?: number;
}

export interface SearchFacetsDto {
  brands: SearchFacetValueDto[];
  sizes: SearchFacetValueDto[];
  colors: SearchFacetValueDto[];
  priceRange: SearchPriceRangeFacetDto;
  availability: SearchFacetValueDto[];
  sale: SearchFacetValueDto[];
  new: SearchFacetValueDto[];
}

export interface SearchProductItemDto {
  productId: number;
  slug: string;
  brandName: string;
  name: string;
  shortDescription?: string;
  primaryCategory?: string;
  secondaryCategories: string[];
  primaryColorName?: string;
  isNew: boolean;
  isBestseller: boolean;
  isOnSale: boolean;
  minPrice?: number;
  maxPrice?: number;
  availableSizes: number[];
  inStock: boolean;
  primaryImageUrl?: string;
  publishedAtUtc?: string;
  sortRank: number;
}

export interface SearchResponseDto {
  products: SearchProductItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  facets: SearchFacetsDto;
  pagination?: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

export interface ProductAutocompleteItemDto {
  productId: number;
  slug: string;
  name: string;
  brandName: string;
  primaryImageUrl?: string;
}

export interface ProductAutocompleteResultDto {
  items: ProductAutocompleteItemDto[];
}

// Home Page
export interface HomePageDto {
  title: string;
  slug: string;
  isPublished: boolean;
  modules: any[];
  seo?: SeoDto;
}

// Brand Page
export interface BrandPageDto {
  id: number;
  slug: string;
  name: string;
  shortDescription?: string;
  longDescription?: string;
  logoUrl?: string;
  coverImageUrl?: string;
  websiteUrl?: string;
  isFeatured: boolean;
  isActive: boolean;
  categories: Array<{ id: number; slug: string; name: string }>;
  featuredProducts: ProductCardDto[];
  seo?: SeoDto;
  intro?: string;
  faq?: any;
  content?: string;
}

// Collection Page
export interface CollectionPageDto {
  id: number;
  slug: string;
  name: string;
  shortDescription?: string;
  longDescription?: string;
  badgeText?: string;
  coverImageUrl?: string;
  thumbnailImageUrl?: string;
  collectionType: number;
  isActive: boolean;
  isFeatured: boolean;
  startAtUtc?: string;
  endAtUtc?: string;
  featuredProducts: ProductCardDto[];
  seo?: SeoDto;
  intro?: string;
  faq?: any;
  merchBlocks?: any;
}

// Editorial
export interface EditorialArticleCardDto {
  id: number;
  slug: string;
  title: string;
  excerpt: string;
  coverImageUrl?: string;
  authorName?: string;
  publishedAtUtc?: string;
  topic?: string;
  readingTimeMinutes?: number;
}

export interface EditorialArticleDto extends EditorialArticleCardDto {
  body: string;
  seo?: SeoDto;
  relatedArticles?: EditorialArticleCardDto[];
  relatedProducts?: ProductCardDto[];
}

// Stores
export interface StoreCardDto {
  id: number;
  slug: string;
  name: string;
  city: string;
  address: string;
  addressLine1?: string;
  addressLine2?: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  hoursOpen?: string;
  hoursClose?: string;
  workingHoursText?: string;
  coverImageUrl?: string;
  imageUrl?: string;
  operatingHours?: string;
  description?: string;
  shortDescription?: string;
  amenities?: string[];
  seo?: SeoDto;
}

export interface StorePageDto extends StoreCardDto {
  addressLine1: string;
  addressLine2?: string;
  postalCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  directionsUrl?: string;
  hoursOverlay?: string;
  isFeatured: boolean;
  phone?: string;
  email?: string;
  seo?: SeoDto;
}

// Cart
export interface CartItemDto {
  id?: number;
  itemId: number;
  productVariantId: number;
  productSlug: string;
  productName: string;
  brandName: string;
  sizeEu: number;
  primaryImageUrl?: string;
  productImageUrl?: string;
  selectedSize?: number | string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  totalPrice?: number;
  isAvailable: boolean;
  isLowStock: boolean;
}

export interface CartDto {
  cartToken: string;
  currency: string;
  items: CartItemDto[];
  totalItems: number;
  totalAmount: number;
}

export interface AddToCartPayload {
  productVariantId: number;
  quantity: number;
}

export interface UpdateCartItemPayload {
  quantity: number;
}

// Checkout
export interface CartItemSummaryDto {
  productName: string;
  brandName: string;
  sizeEu: number;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CheckoutSummaryDto {
  cartToken: string;
  items: CartItemSummaryDto[];
  subtotalAmount: number;
  deliveryAmount: number;
  totalAmount: number;
  itemCount: number;
}

export interface CheckoutRequest {
  idempotencyKey?: string;
  cartToken: string;
  customerFirstName: string;
  customerLastName: string;
  email: string;
  customerEmail?: string;
  phone: string;
  deliveryAddressLine1: string;
  deliveryAddressLine2?: string;
  deliveryCity: string;
  deliveryPostalCode: string;
  deliveryMethod: string;
  paymentMethod: string;
  note?: string;
}

export interface CheckoutResultDto {
  outcome: 'Success' | 'AlreadyProcessed' | 'InvalidCart' | 'InsufficientStock';
  message: string;
  orderNumber: string;
  totalAmount: number;
  status: string;
  isSuccess?: boolean;
  alreadyProcessed?: boolean;
}

export type DeliveryMethod = 'Courier' | 'StorePickup';
export type PaymentMethod = 'CashOnDelivery' | 'CardPlaceholder';

// Order
export interface OrderItemDto {
  productName: string;
  brandName: string;
  sizeEu: number;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderDto {
  orderNumber: string;
  status: string;
  customerFullName: string;
  email: string;
  phone: string;
  deliveryAddressLine1: string;
  deliveryAddressLine2?: string;
  deliveryCity: string;
  deliveryPostalCode: string;
  deliveryMethod: string;
  paymentMethod: string;
  subtotalAmount: number;
  deliveryAmount: number;
  totalAmount: number;
  items: OrderItemDto[];
  createdAt: string;
  placedAt?: string;
}

// Wishlist
export interface WishlistItemDto {
  productId: number;
  productSlug: string;
  productName: string;
  brandName: string;
  primaryImageUrl?: string;
  price: number;
  oldPrice?: number;
  isInStock: boolean;
  addedAtUtc: string;
}

export interface WishlistDto {
  wishlistToken: string;
  items: WishlistItemDto[];
  itemCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AddToWishlistRequest {
  productId: number;
}
