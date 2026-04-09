import { apiClient } from './api-client';
import type {
  WishlistDto,
  AddToWishlistRequest,
} from '@/lib/types';

export async function createWishlist() {
  return apiClient.post<WishlistDto>('/wishlist', {});
}

export async function getWishlist(wishlistToken: string) {
  return apiClient.get<WishlistDto>(`/wishlist/${wishlistToken}`);
}

export async function addToWishlist(
  wishlistToken: string,
  request: AddToWishlistRequest
) {
  return apiClient.post<WishlistDto>(
    `/wishlist/${wishlistToken}/items`,
    request
  );
}

export async function removeFromWishlist(
  wishlistToken: string,
  productId: number
) {
  return apiClient.delete<WishlistDto>(
    `/wishlist/${wishlistToken}/items/${productId}`
  );
}

export async function clearWishlist(wishlistToken: string) {
  return apiClient.delete<WishlistDto>(
    `/wishlist/${wishlistToken}/items`
  );
}
