// Client-side cart token storage using localStorage
const CART_TOKEN_KEY = 'trendplus-cart-token';

export function getCartToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(CART_TOKEN_KEY);
}

export function setCartToken(token: string): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(CART_TOKEN_KEY, token);
}

export function clearCartToken(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(CART_TOKEN_KEY);
}

export function hasCartToken(): boolean {
  return getCartToken() !== null;
}
