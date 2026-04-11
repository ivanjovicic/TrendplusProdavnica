'use client';

import { useState, useCallback } from 'react';
import * as cartApi from '@/lib/api/cart';
import { getCartToken, setCartToken } from '@/lib/utils/cart-storage';

interface AddToCartButtonProps {
  variantId: number;
  quantity?: number;
  onSuccess?: () => void;
  onSizeRequired?: () => void;
}

export function AddToCartButton({ variantId, quantity = 1, onSuccess, onSizeRequired }: AddToCartButtonProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleAddToCart = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setSuccessMessage(null);

      if (variantId <= 0) {
        onSizeRequired?.();
        return;
      }

      let token = getCartToken();

      if (!token) {
        const newCart = await cartApi.createCart();
        token = newCart.cartToken;
        setCartToken(token);
      }

      await cartApi.addToCart(token, { productVariantId: variantId, quantity });

      setSuccessMessage('Dodano u korpu! ✓');
      
      // Clear success message after 3 seconds
      setTimeout(() => setSuccessMessage(null), 3000);
      
      onSuccess?.();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Došlo je do greške pri dodavanju u korpu';
      setError(errorMessage);
      console.error('Add to cart error:', err);
    } finally {
      setLoading(false);
    }
  }, [onSuccess, quantity, variantId, onSizeRequired]);

  return (
    <div className="space-y-3">
      <button
        onClick={handleAddToCart}
        disabled={loading || variantId <= 0}
        className="w-full rounded-lg bg-black py-3 text-white font-medium transition-colors hover:bg-gray-800 disabled:bg-gray-400 disabled:cursor-not-allowed"
      >
        {loading ? 'Dodajem...' : 'Dodaj u korpu'}
      </button>
      
      {error && (
        <div className="rounded-lg bg-red-50 p-3 border border-red-200">
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}
      
      {successMessage && (
        <div className="rounded-lg bg-green-50 p-3 border border-green-200">
          <p className="text-sm text-green-700 font-medium">{successMessage}</p>
        </div>
      )}
    </div>
  );
}
