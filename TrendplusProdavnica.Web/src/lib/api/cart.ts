import { apiClient } from './api-client';
import type { CartDto, AddToCartPayload, UpdateCartItemPayload } from '@/lib/types';

export async function createCart() {
  return apiClient.post<CartDto>('/cart');
}

export async function getCart(cartToken: string) {
  return apiClient.get<CartDto>(`/cart/${cartToken}`);
}

export async function addToCart(cartToken: string, payload: AddToCartPayload) {
  return apiClient.post<CartDto>(`/cart/${cartToken}/items`, payload);
}

export async function updateCartItem(cartToken: string, itemId: number, payload: UpdateCartItemPayload) {
  return apiClient.patch<CartDto>(`/cart/${cartToken}/items/${itemId}`, payload);
}

export async function removeCartItem(cartToken: string, itemId: number) {
  return apiClient.delete<CartDto>(`/cart/${cartToken}/items/${itemId}`);
}

export async function clearCart(cartToken: string) {
  return apiClient.delete<CartDto>(`/cart/${cartToken}/items`);
}
