'use client';

import { useState } from 'react';
import * as wishlistApi from '@/lib/api/wishlist';
import { getWishlistToken, setWishlistToken } from '@/lib/utils/wishlist-storage';

interface AddToWishlistButtonProps {
  productId: number;
  className?: string;
  showText?: boolean;
}

export function AddToWishlistButton({
  productId,
  className = '',
  showText = false,
}: AddToWishlistButtonProps) {
  const [loading, setLoading] = useState(false);
  const [isAdded, setIsAdded] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddToWishlist = async () => {
    setLoading(true);
    setError(null);

    try {
      let token = getWishlistToken();

      if (!token) {
        const newWishlist = await wishlistApi.createWishlist();
        token = newWishlist.wishlistToken;
        setWishlistToken(token);
      }

      await wishlistApi.addToWishlist(token, { productId });
      setIsAdded(true);

      setTimeout(() => setIsAdded(false), 2000);
    } catch (err: any) {
      setError(err.message || 'Ne mogu da dodam u listu zelja');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <button
        onClick={handleAddToWishlist}
        disabled={loading || isAdded}
        className={`
          inline-flex items-center justify-center gap-2
          transition-colors duration-200
          ${isAdded ? 'text-red-600' : 'text-gray-600 hover:text-red-600'}
          disabled:cursor-not-allowed disabled:opacity-50
          ${className}
        `}
        title={isAdded ? 'Dodano u listu zelja!' : 'Dodaj u listu zelja'}
      >
        <svg
          className={`h-5 w-5 ${isAdded ? 'fill-current' : ''}`}
          fill={isAdded ? 'currentColor' : 'none'}
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
          />
        </svg>
        {showText && <span className="text-sm font-medium">{isAdded ? 'Dodano!' : 'Zelje'}</span>}
      </button>
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
}
