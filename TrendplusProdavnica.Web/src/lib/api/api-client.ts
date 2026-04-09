const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'https://localhost:7002/api';

export interface ApiError {
  error: string;
  message: string;
  statusCode?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: ApiError;
}

class ApiClient {
  private baseUrl = API_BASE_URL;

  async fetch<T>(
    endpoint: string,
    options?: RequestInit & { searchParams?: Record<string, string | number | boolean | undefined> }
  ): Promise<T> {
    const { searchParams, ...fetchOptions } = options || {};
    
    let url = `${this.baseUrl}${endpoint}`;
    
    if (searchParams) {
      const params = new URLSearchParams();
      Object.entries(searchParams).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, String(value));
        }
      });
      if (params.toString()) {
        url += `?${params.toString()}`;
      }
    }

    const response = await fetch(url, {
      ...fetchOptions,
      headers: {
        'Content-Type': 'application/json',
        ...fetchOptions?.headers,
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({
        error: 'Unknown error',
        message: response.statusText,
        statusCode: response.status,
      }));
      throw {
        ...error,
        statusCode: response.status,
      } as ApiError;
    }

    return response.json();
  }

  async get<T>(endpoint: string, options?: RequestInit & { searchParams?: Record<string, any> }): Promise<T> {
    return this.fetch<T>(endpoint, { ...options, method: 'GET' });
  }

  async post<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this.fetch<T>(endpoint, {
      ...options,
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  async patch<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this.fetch<T>(endpoint, {
      ...options,
      method: 'PATCH',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  async delete<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.fetch<T>(endpoint, { ...options, method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();
