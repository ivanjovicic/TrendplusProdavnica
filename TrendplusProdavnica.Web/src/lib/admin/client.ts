// Admin API client service for Next.js

export interface AdminAuthToken {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: AdminUser;
}

export interface AdminUser {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
  lastLoginUtc?: string;
}

export interface ProductListItem {
  id: number;
  name: string;
  slug: string;
  brandName: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdUtc: string;
  imageUrl?: string;
}

export interface OrderListItem {
  id: number;
  orderNumber: string;
  customerEmail: string;
  customerName: string;
  totalAmount: number;
  status: string;
  createdUtc: string;
  itemCount: number;
}

export interface BrandListItem {
  id: number;
  name: string;
  logoUrl?: string;
  isActive: boolean;
  productCount: number;
}

export interface CollectionListItem {
  id: number;
  name: string;
  slug: string;
  thumbnailUrl?: string;
  isActive: boolean;
  productCount: number;
}

export interface EditorialListItem {
  id: number;
  title: string;
  slug: string;
  thumbnailUrl?: string;
  isPublished: boolean;
  viewCount?: number;
  publishedUtc: string;
}

export interface AdminListResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasMore: boolean;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

type CanonicalAdminListResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

type CanonicalProductListItem = {
  id: number;
  name: string;
  slug: string;
  brandName: string;
  price?: number | null;
  stockQuantity?: number;
  isActive?: boolean;
  updatedAtUtc?: string;
};

type CanonicalBrandListItem = {
  id: number;
  name: string;
  logoUrl?: string | null;
  isActive: boolean;
  productCount?: number;
};

type CanonicalCollectionListItem = {
  id: number;
  name: string;
  slug: string;
  thumbnailImageUrl?: string | null;
  isActive: boolean;
  productCount?: number;
};

type CanonicalEditorialListItem = {
  id: number;
  title: string;
  slug: string;
  coverImageUrl?: string | null;
  status: string | number;
  publishedAtUtc?: string | null;
};

type CanonicalHomePage = {
  id: number;
  title: string;
  slug: string;
  modules?: Array<{ type: string; payload?: unknown }>;
};

function normalizeListResponse<T, TOut>(
  response: CanonicalAdminListResponse<T>,
  mapItem: (item: T) => TOut
): AdminListResponse<TOut> {
  return {
    items: response.items.map(mapItem),
    totalCount: response.totalCount,
    page: response.page,
    pageSize: response.pageSize,
    totalPages: response.totalPages,
    hasMore: response.page < response.totalPages,
  };
}

class AdminApiClient {
  private token: string | null = null;

  setToken(token: string | null) {
    this.token = token;
    if (token) {
      localStorage.setItem("adminToken", token);
    } else {
      localStorage.removeItem("adminToken");
    }
  }

  getToken(): string | null {
    if (this.token) return this.token;
    if (typeof window !== "undefined") {
      return localStorage.getItem("adminToken");
    }
    return null;
  }

  private getHeaders(includeAuth = true): Record<string, string> {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };

    if (includeAuth && this.getToken()) {
      headers.Authorization = `Bearer ${this.getToken()}`;
    }

    return headers;
  }

  private async request<T>(
    endpoint: string,
    method: "GET" | "POST" | "PUT" | "DELETE" = "GET",
    body?: unknown,
    includeAuth = true
  ): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;
    const options: RequestInit = {
      method,
      headers: this.getHeaders(includeAuth),
    };

    if (body) {
      options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);

    if (!response.ok) {
      if (response.status === 401) {
        this.setToken(null);
        throw new Error("Unauthorized");
      }
      throw new Error(`API error: ${response.statusText}`);
    }

    if (response.status === 204) {
      return null as T;
    }

    return response.json();
  }

  // Auth endpoints
  async login(email: string, password: string): Promise<AdminAuthToken> {
    const result = await this.request<AdminAuthToken>(
      "/api/admin/auth/login",
      "POST",
      { email, password },
      false
    );
    this.setToken(result.token);
    return result;
  }

  async refreshToken(refreshToken: string): Promise<AdminAuthToken> {
    const result = await this.request<AdminAuthToken>(
      "/api/admin/auth/refresh",
      "POST",
      { refreshToken },
      false
    );
    this.setToken(result.token);
    return result;
  }

  // Product endpoints
  async getProducts(
    page = 1,
    pageSize = 20,
    search?: string
  ): Promise<AdminListResponse<ProductListItem>> {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    if (search) params.append("search", search);
    const result = await this.request<CanonicalAdminListResponse<CanonicalProductListItem>>(
      `/api/admin/products?${params}`
    );

    return normalizeListResponse(result, (item) => ({
      id: item.id,
      name: item.name,
      slug: item.slug,
      brandName: item.brandName,
      price: item.price ?? 0,
      stockQuantity: item.stockQuantity ?? 0,
      isActive: item.isActive ?? false,
      createdUtc: item.updatedAtUtc ?? new Date(0).toISOString(),
    }));
  }

  async getProduct(id: number) {
    return this.request(`/api/admin/products/${id}`);
  }

  async createProduct(data: unknown) {
    return this.request("/api/admin/products", "POST", data);
  }

  async updateProduct(id: number, data: unknown) {
    return this.request(`/api/admin/products/${id}`, "PUT", data);
  }

  async deleteProduct(id: number) {
    return this.request(`/api/admin/products/${id}`, "DELETE");
  }

  // Order endpoints
  async getOrders(
    page = 1,
    pageSize = 20,
    status?: string
  ): Promise<AdminListResponse<OrderListItem>> {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    if (status) params.append("status", status);
    const result = await this.request<CanonicalAdminListResponse<OrderListItem>>(
      `/api/admin/orders?${params}`
    );

    return normalizeListResponse(result, (item) => ({
      ...item,
      status: item.status.toLowerCase(),
      itemCount: item.itemCount ?? 0,
    }));
  }

  async getOrder(id: number) {
    return this.request(`/api/admin/orders/${id}`);
  }

  async updateOrderStatus(id: number, status: string, notes?: string) {
    return this.request(`/api/admin/orders/${id}/status`, "PUT", {
      status,
      notes,
    });
  }

  // Brand endpoints
  async getBrands(
    page = 1,
    pageSize = 20
  ): Promise<AdminListResponse<BrandListItem>> {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    const result = await this.request<CanonicalAdminListResponse<CanonicalBrandListItem>>(
      `/api/admin/brands?${params}`
    );

    return normalizeListResponse(result, (item) => ({
      id: item.id,
      name: item.name,
      logoUrl: item.logoUrl ?? undefined,
      isActive: item.isActive,
      productCount: item.productCount ?? 0,
    }));
  }

  // Collection endpoints
  async getCollections(
    page = 1,
    pageSize = 20
  ): Promise<AdminListResponse<CollectionListItem>> {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    const result = await this.request<CanonicalAdminListResponse<CanonicalCollectionListItem>>(
      `/api/admin/collections?${params}`
    );

    return normalizeListResponse(result, (item) => ({
      id: item.id,
      name: item.name,
      slug: item.slug,
      thumbnailUrl: item.thumbnailImageUrl ?? undefined,
      isActive: item.isActive,
      productCount: item.productCount ?? 0,
    }));
  }

  // Editorial endpoints
  async getEditorialArticles(
    page = 1,
    pageSize = 20,
    published?: boolean
  ): Promise<AdminListResponse<EditorialListItem>> {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    if (published !== undefined) params.append("published", published.toString());
    const result = await this.request<CanonicalAdminListResponse<CanonicalEditorialListItem>>(
      `/api/admin/editorial?${params}`
    );

    return normalizeListResponse(result, (item) => ({
      id: item.id,
      title: item.title,
      slug: item.slug,
      thumbnailUrl: item.coverImageUrl ?? undefined,
      isPublished: item.status === "Published" || item.status === 1,
      publishedUtc: item.publishedAtUtc ?? new Date(0).toISOString(),
    }));
  }

  // Homepage endpoints
  async getHomepageConfig() {
    const result = await this.request<CanonicalHomePage>("/api/admin/home-page");

    return {
      id: result.id,
      title: result.title,
      slug: result.slug,
      sections: (result.modules ?? []).map((module, index) => ({
        id: `${index + 1}`,
        type: module.type,
        isActive: true,
        payload: module.payload,
      })),
    };
  }
}

export const adminApiClient = new AdminApiClient();
