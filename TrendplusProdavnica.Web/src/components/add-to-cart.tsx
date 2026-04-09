'use client';

import { useState, useCallback } from 'react';
import * as cartApi from '@/lib/api/cart';
import { getCartToken, setCartToken } from '@/lib/utils/cart-storage';

interface AddToCartButtonProps {
  variantId: number;
  quantity?: number;
  onSuccess?: () => void;
}

export function AddToCartButton({ variantId, quantity = 1, onSuccess }: AddToCartButtonProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddToCart = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      let token = getCartToken();
      
      if (!token) {
        const newCart = await cartApi.createCart();
        token = newCart.cartToken;
        setCartToken(token);
      }
      
      await cartApi.addToCart(token, { productVariantId: variantId, quantity });
      
      onSuccess?.();
      alert('Dodano u korpu!');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Došlo je do greške');
      console.error('Add to cart error:', err);
    } finally {
      setLoading(false);
    }
  }, [variantId, onSuccess]);

  return (
    <div className="space-y-2">
      <button
        onClick={handleAddToCart}
        disabled={loading}
        className="w-full bg-black text-white py-3 rounded-lg hover:bg-gray-800 disabled:bg-gray-400 transition-colors"
      >
        {loading ? 'Dodajem...' : 'Dodaj u korpu'}
      </button>
      {error && <p className="text-red-600 text-sm">{error}</p>}
    </div>
  );
}
