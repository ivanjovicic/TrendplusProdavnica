'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import * as wishlistApi from '@/lib/api/wishlist';
import { getWishlistToken, setWishlistToken } from '@/lib/utils/wishlist-storage';
import { formatPrice } from '@/lib/utils/helpers';
import { WishlistDto, WishlistItemDto } from '@/lib/types';

export default function WishlistPage() {
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadWishlist = async () => {
      try {
        let token = getWishlistToken();

        if (!token) {
          const newWishlist = await wishlistApi.createWishlist();
          token = newWishlist.wishlistToken;
          setWishlistToken(token);
        }

        const data = await wishlistApi.getWishlist(token);
        if (data) {
          setWishlist(data);
        }
      } catch (err) {
        setError('Ne mogu da ucitam listu zelja');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadWishlist();
  }, []);

  const handleRemoveItem = async (productId: number) => {
    try {
      if (!wishlist) return;
      const updated = await wishlistApi.removeFromWishlist(wishlist.wishlistToken, productId);
      setWishlist(updated);
    } catch (err) {
      setError('Ne mogu da obrisem proizvod');
      console.error(err);
    }
  };

  const handleClear = async () => {
    if (!confirm('Da li ste sigurni da zelite da obrisete celu listu?')) {
      return;
    }

    try {
      if (!wishlist) return;
      const updated = await wishlistApi.clearWishlist(wishlist.wishlistToken);
      setWishlist(updated);
    } catch (err) {
      setError('Ne mogu da obrisem listu');
      console.error(err);
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <p className="text-gray-600">Ucitavanje...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      <div className="mx-auto max-w-7xl px-4 py-8">
        <h1 className="mb-8 text-4xl font-bold">Lista zelja</h1>

        {error && (
          <div className="mb-8 rounded border border-red-400 bg-red-100 px-4 py-3 text-red-700">
            {error}
          </div>
        )}

        {!wishlist || wishlist.items.length === 0 ? (
          <div className="py-16 text-center">
            <h2 className="mb-4 text-2xl font-semibold">Lista zelja je prazna</h2>
            <p className="mb-8 text-gray-600">Pocnite dodavanjem proizvoda u listu zelja</p>
            <Link
              href="/"
              className="inline-block rounded bg-black px-6 py-3 font-semibold text-white hover:bg-gray-800"
            >
              Pogledajte proizvode
            </Link>
          </div>
        ) : (
          <div>
            <div className="mb-6 flex items-center justify-between">
              <p className="text-gray-600">
                {wishlist.itemCount} proizvod{wishlist.itemCount !== 1 ? 'a' : ''}
              </p>
              {wishlist.items.length > 0 && (
                <button
                  onClick={handleClear}
                  className="text-sm font-semibold text-red-600 hover:text-red-800"
                >
                  Obrisi sve
                </button>
              )}
            </div>

            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
              {wishlist.items.map((item) => (
                <WishlistProductCard
                  key={item.productId}
                  item={item}
                  onRemove={() => handleRemoveItem(item.productId)}
                />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function WishlistProductCard({
  item,
  onRemove,
}: {
  item: WishlistItemDto;
  onRemove: () => void;
}) {
  return (
    <div className="overflow-hidden rounded-lg border transition-shadow hover:shadow-lg">
      {item.primaryImageUrl && (
        <Link href={`/proizvod/${item.productSlug}`}>
          <div className="relative aspect-square overflow-hidden bg-gray-200">
            <img
              src={item.primaryImageUrl}
              alt={item.productName}
              className="h-full w-full object-cover transition-transform hover:scale-105"
            />
            {!item.isInStock && (
              <div className="absolute inset-0 flex items-center justify-center bg-black/50">
                <span className="font-semibold text-white">Nema na zalihi</span>
              </div>
            )}
          </div>
        </Link>
      )}

      <div className="p-4">
        <Link href={`/proizvod/${item.productSlug}`}>
          <h3 className="line-clamp-2 font-semibold hover:text-blue-600">{item.productName}</h3>
        </Link>

        <p className="mb-3 text-sm text-gray-600">{item.brandName}</p>

        <div className="mb-4 flex items-center justify-between">
          <div>
            <span className="text-lg font-bold">{formatPrice(item.price)}</span>
            {item.oldPrice && item.oldPrice > item.price && (
              <span className="ml-2 text-sm text-gray-500 line-through">{formatPrice(item.oldPrice)}</span>
            )}
          </div>
        </div>

        <div className="flex gap-2">
          <Link
            href={`/proizvod/${item.productSlug}`}
            className="flex-1 rounded bg-black py-2 text-center text-sm font-semibold text-white hover:bg-gray-800"
          >
            Vidite detalje
          </Link>
          <button
            onClick={onRemove}
            className="rounded border border-gray-300 px-3 py-2 hover:border-red-600 hover:text-red-600"
            title="Obrisi iz liste"
          >
            x
          </button>
        </div>
      </div>
    </div>
  );
}
