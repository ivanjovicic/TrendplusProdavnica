'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import * as wishlistApi from '@/lib/api/wishlist';
import { getWishlistToken, setWishlistToken, clearWishlistToken } from '@/lib/utils/wishlist-storage';
import { formatPrice } from '@/lib/utils/helpers';
import { WishlistDto, WishlistItemDto } from '@/lib/types';

export default function WishlistPage() {
  const router = useRouter();
  const [wishlist, setWishlist] = useState<WishlistDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadWishlist = async () => {
      try {
        let token = getWishlistToken();

        // If no token, create new wishlist
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
        setError('Не могу да учитам листу жеља');
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
      setError('Не могу да обришем производ');
      console.error(err);
    }
  };

  const handleClear = async () => {
    if (!confirm('Да ли сте сигурни да желите да обришете сву листу?')) {
      return;
    }
    try {
      if (!wishlist) return;
      const updated = await wishlistApi.clearWishlist(wishlist.wishlistToken);
      setWishlist(updated);
    } catch (err) {
      setError('Не могу да обришем листу');
      console.error(err);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Учитавање...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-4xl font-bold mb-8">Листа жеља</h1>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-8">
            {error}
          </div>
        )}

        {!wishlist || wishlist.items.length === 0 ? (
          <div className="text-center py-16">
            <h2 className="text-2xl font-semibold mb-4">Листа жеља је празна</h2>
            <p className="text-gray-600 mb-8">
              Почните додавањем производа у листу жеља
            </p>
            <Link
              href="/"
              className="inline-block bg-black text-white px-6 py-3 rounded font-semibold hover:bg-gray-800"
            >
              Погледајте производе
            </Link>
          </div>
        ) : (
          <div>
            <div className="flex justify-between items-center mb-6">
              <p className="text-gray-600">{wishlist.itemCount} производ{wishlist.itemCount !== 1 ? 'а' : ''}</p>
              {wishlist.items.length > 0 && (
                <button
                  onClick={handleClear}
                  className="text-red-600 hover:text-red-800 text-sm font-semibold"
                >
                  Обриши све
                </button>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
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
    <div className="border rounded-lg overflow-hidden hover:shadow-lg transition-shadow">
      {item.primaryImageUrl && (
        <Link href={`/proizvod/${item.productSlug}`}>
          <div className="relative bg-gray-200 aspect-square overflow-hidden">
            <img
              src={item.primaryImageUrl}
              alt={item.productName}
              className="w-full h-full object-cover hover:scale-105 transition-transform"
            />
            {!item.isInStock && (
              <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
                <span className="text-white font-semibold">Нема на залихи</span>
              </div>
            )}
          </div>
        </Link>
      )}

      <div className="p-4">
        <Link href={`/proizvod/${item.productSlug}`}>
          <h3 className="font-semibold hover:text-blue-600 line-clamp-2">
            {item.productName}
          </h3>
        </Link>

        <p className="text-sm text-gray-600 mb-3">{item.brandName}</p>

        <div className="flex items-center justify-between mb-4">
          <div>
            <span className="text-lg font-bold">{formatPrice(item.price)}</span>
            {item.oldPrice && item.oldPrice > item.price && (
              <span className="text-sm line-through text-gray-500 ml-2">
                {formatPrice(item.oldPrice)}
              </span>
            )}
          </div>
        </div>

        <div className="flex gap-2">
          <Link
            href={`/proizvod/${item.productSlug}`}
            className="flex-1 bg-black text-white py-2 rounded text-center text-sm font-semibold hover:bg-gray-800"
          >
            Видите детаље
          </Link>
          <button
            onClick={onRemove}
            className="px-3 py-2 border border-gray-300 rounded hover:border-red-600 hover:text-red-600"
            title="Обриши из листе"
          >
            ✕
          </button>
        </div>
      </div>
    </div>
  );
}
