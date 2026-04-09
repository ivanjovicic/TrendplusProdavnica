const WISHLIST_TOKEN_KEY = 'wishlist_token';

export function getWishlistToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(WISHLIST_TOKEN_KEY);
}

export function setWishlistToken(token: string): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(WISHLIST_TOKEN_KEY, token);
}

export function clearWishlistToken(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(WISHLIST_TOKEN_KEY);
}
