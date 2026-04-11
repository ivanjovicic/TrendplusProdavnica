'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import * as cartApi from '@/lib/api/cart';
import { clearCartToken, getCartToken } from '@/lib/utils/cart-storage';
import { formatPrice } from '@/lib/utils/helpers';
import type { CartDto, CartItemDto } from '@/lib/types';

export default function CartPage() {
  const [cart, setCart] = useState<CartDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadCart = async () => {
      try {
        const token = getCartToken();
        if (!token) {
          setCart(null);
          return;
        }

        const data = await cartApi.getCart(token);
        setCart(data);
      } catch (err) {
        console.error(err);
        setError('Ne mogu da ucitam korpu.');
      } finally {
        setLoading(false);
      }
    };

    void loadCart();
  }, []);

  async function refreshCart(cartToken: string) {
    const updated = await cartApi.getCart(cartToken);
    setCart(updated);
  }

  async function handleUpdateQuantity(itemId: number, quantity: number) {
    if (!cart || quantity < 1) {
      return;
    }

    try {
      await cartApi.updateCartItem(cart.cartToken, itemId, { quantity });
      await refreshCart(cart.cartToken);
    } catch (err) {
      console.error(err);
      setError('Ne mogu da azuriram kolicinu.');
    }
  }

  async function handleRemoveItem(itemId: number) {
    if (!cart) {
      return;
    }

    try {
      await cartApi.removeCartItem(cart.cartToken, itemId);
      await refreshCart(cart.cartToken);
    } catch (err) {
      console.error(err);
      setError('Ne mogu da uklonim stavku iz korpe.');
    }
  }

  async function handleClearCart() {
    if (!cart) {
      return;
    }

    if (!window.confirm('Da li sigurno zelis da obrises sve stavke iz korpe?')) {
      return;
    }

    try {
      await cartApi.clearCart(cart.cartToken);
      clearCartToken();
      setCart(null);
    } catch (err) {
      console.error(err);
      setError('Ne mogu da ispraznim korpu.');
    }
  }

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <p className="text-gray-600">Ucitavanje korpe...</p>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-4xl px-4 py-8">
          <h1 className="mb-8 text-4xl font-bold">Korpa</h1>

          <div className="py-16 text-center">
            <h2 className="mb-4 text-2xl font-bold">Korpa je prazna</h2>
            <p className="mb-8 text-gray-600">Dodajte proizvode koje zelite da kupite.</p>
            <Link href="/" className="inline-block rounded bg-black px-6 py-3 text-white hover:bg-gray-800">
              Nastavi kupovinu
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const totalItems = cart.items.reduce((sum, item) => sum + item.quantity, 0);
  const totalAmount = cart.totalAmount;

  return (
    <div className="min-h-screen bg-white">
      <div className="mx-auto max-w-5xl px-4 py-8">
        <h1 className="mb-8 text-4xl font-bold">Korpa</h1>

        {error && (
          <div className="mb-8 rounded border border-red-300 bg-red-50 px-4 py-3 text-red-700">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-4 lg:col-span-2">
            {cart.items.map((item) => (
              <CartItemRow
                key={item.itemId}
                item={item}
                onQuantityChange={handleUpdateQuantity}
                onRemove={handleRemoveItem}
              />
            ))}

            <div className="mt-8 flex flex-wrap gap-4">
              <Link href="/" className="rounded border border-black px-6 py-2 text-black hover:bg-gray-100">
                Nastavi kupovinu
              </Link>
              <button
                onClick={handleClearCart}
                className="rounded border border-red-600 px-6 py-2 text-red-600 hover:bg-red-50"
              >
                Isprazni korpu
              </button>
            </div>
          </div>

          <aside className="lg:col-span-1">
            <div className="sticky top-4 rounded-lg bg-gray-50 p-6">
              <h2 className="mb-6 text-2xl font-bold">Rezime porudzbine</h2>

              <div className="mb-6 space-y-4 border-b pb-6">
                <div className="flex justify-between">
                  <span>Brendova:</span>
                  <span className="font-semibold">{new Set(cart.items.map((item) => item.brandName)).size}</span>
                </div>
                <div className="flex justify-between">
                  <span>Stavki:</span>
                  <span className="font-semibold">{cart.items.length}</span>
                </div>
                <div className="flex justify-between">
                  <span>Kolicina:</span>
                  <span className="font-semibold">{totalItems}</span>
                </div>
              </div>

              <div className="mb-6 flex justify-between text-xl font-bold">
                <span>Ukupno:</span>
                <span>{formatPrice(totalAmount)}</span>
              </div>

              <Link
                href="/checkout"
                className="block w-full rounded bg-black py-3 text-center font-semibold text-white hover:bg-gray-800"
              >
                Nastavi na checkout
              </Link>

              <p className="mt-4 text-center text-xs text-gray-600">
                Dostava i finalni troskovi bice prikazani u checkout koraku.
              </p>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}

function CartItemRow({
  item,
  onQuantityChange,
  onRemove,
}: {
  item: CartItemDto;
  onQuantityChange: (itemId: number, quantity: number) => Promise<void>;
  onRemove: (itemId: number) => Promise<void>;
}) {
  const [updating, setUpdating] = useState(false);
  const imageUrl = item.primaryImageUrl || item.productImageUrl;
  const sizeLabel = item.selectedSize ?? item.sizeEu;
  const lineTotal = item.totalPrice ?? item.lineTotal;

  async function handleQuantityChange(nextQuantity: number) {
    if (nextQuantity < 1) {
      return;
    }

    setUpdating(true);
    try {
      await onQuantityChange(item.itemId, nextQuantity);
    } finally {
      setUpdating(false);
    }
  }

  async function handleRemove() {
    setUpdating(true);
    try {
      await onRemove(item.itemId);
    } finally {
      setUpdating(false);
    }
  }

  return (
    <div className="flex gap-4 rounded-lg border p-4">
      {imageUrl && (
        <img
          src={imageUrl}
          alt={item.productName}
          className="h-20 w-20 rounded object-cover"
        />
      )}

      <div className="min-w-0 flex-1">
        <h3 className="truncate text-lg font-semibold">{item.productName}</h3>
        <p className="text-sm text-gray-600">{item.brandName}</p>
        <p className="text-sm text-gray-600">Velicina: {sizeLabel}</p>
      </div>

      <div className="text-right">
        <div className="mb-4 font-semibold">{formatPrice(item.unitPrice)}</div>

        <div className="mb-4 flex items-center gap-2">
          <button
            onClick={() => void handleQuantityChange(item.quantity - 1)}
            disabled={updating || item.quantity <= 1}
            className="rounded border px-2 py-1 hover:bg-gray-100 disabled:opacity-50"
          >
            -
          </button>
          <input
            type="number"
            min={1}
            value={item.quantity}
            onChange={(event) => void handleQuantityChange(Number.parseInt(event.target.value || '1', 10))}
            disabled={updating}
            className="w-14 rounded border text-center"
          />
          <button
            onClick={() => void handleQuantityChange(item.quantity + 1)}
            disabled={updating}
            className="rounded border px-2 py-1 hover:bg-gray-100 disabled:opacity-50"
          >
            +
          </button>
        </div>

        <div className="mb-4 text-lg font-bold">{formatPrice(lineTotal)}</div>

        <button
          onClick={() => void handleRemove()}
          disabled={updating}
          className="text-sm font-semibold text-red-600 hover:text-red-800 disabled:opacity-50"
        >
          Ukloni
        </button>
      </div>
    </div>
  );
}
