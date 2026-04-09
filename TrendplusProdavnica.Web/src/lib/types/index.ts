// SEO & Metadata
export interface SeoDto {
  seoTitle?: string;
  seoDescription?: string;
  canonicalUrl?: string;
  robotsDirective?: string;
  ogTitle?: string;
  ogDescription?: string;
  ogImageUrl?: string;
  structuredDataOverrideJson?: string;
}

export interface BreadcrumbItemDto {
  label: string;
  slug: string;
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
  sizeEu: number;
  sku: string;
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
  slug: string;
  name: string;
  subtitle?: string;
  shortDescription: string;
  brandId: number;
  brandName: string;
  primaryColorName?: string;
  primaryImageUrl?: string;
  price: number;
  oldPrice?: number;
  currency: string;
  isNew: boolean;
  isBestseller: boolean;
  isOnSale: boolean;
  availableSizes: number;
  isVisibleInStorefront: boolean;
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
  price: number;
  oldPrice?: number;
  currency: string;
  isNew: boolean;
  isBestseller: boolean;
  isOnSale: boolean;
  media: ProductMediaDto[];
  sizes: ProductSizeOptionDto[];
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
    priceRange?: { min: number; max: number };
  };
  merch?: string;
  faq?: any;
  seo?: SeoDto;
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
  phone?: string;
  email?: string;
  hoursOpen?: string;
  hoursClose?: string;
  coverImageUrl?: string;
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
  itemId: number;
  productVariantId: number;
  productSlug: string;
  productName: string;
  brandName: string;
  sizeEu: number;
  primaryImageUrl?: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
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
  cartToken: string;
  customerFirstName: string;
  customerLastName: string;
  email: string;
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
  orderNumber: string;
  totalAmount: number;
  status: string;
}

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
