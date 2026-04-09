'use client';

import { useEffect, useState } from 'react';
import * as cartApi from '@/lib/api/cart';
import { getCartToken, clearCartToken } from '@/lib/utils/cart-storage';
import { formatPrice } from '@/lib/utils/helpers';
import { CartDto, CartItemDto } from '@/lib/types';

export default function CartPage() {
  const [cart, setCart] = useState<CartDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load cart on mount
  useEffect(() => {
    const loadCart = async () => {
      try {
        const token = getCartToken();
        if (token) {
          const data = await cartApi.getCart(token);
          setCart(data);
        }
      } catch (err) {
        setError('Greška pri učitavanju korpe');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadCart();
  }, []);

  const handleUpdateQuantity = async (itemId: string, quantity: number) => {
    if (!cart) return;

    try {
      await cartApi.updateCartItem(cart.cartToken, itemId, { quantity });
      // Reload cart
      const updated = await cartApi.getCart(cart.cartToken);
      setCart(updated);
    } catch (err) {
      setError('Greška pri ažuriranju stavke');
      console.error(err);
    }
  };

  const handleRemoveItem = async (itemId: string) => {
    if (!cart) return;

    try {
      await cartApi.removeCartItem(cart.cartToken, itemId);
      // Reload cart
      const updated = await cartApi.getCart(cart.cartToken);
      setCart(updated);
    } catch (err) {
      setError('Greška pri uklanjanju stavke');
      console.error(err);
    }
  };

  const handleClearCart = async () => {
    if (!cart) return;

    if (confirm('Da li ste sigurni da želite obrisati sve stavke iz korpe?')) {
      try {
        await cartApi.clearCart(cart.cartToken);
        setCart(null);
        clearCartToken();
      } catch (err) {
        setError('Greška pri brisanju korpe');
        console.error(err);
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div>Učitavanje korpe...</div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-4xl mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold mb-8">Korpa</h1>

          <div className="text-center py-16">
            <h2 className="text-2xl font-bold mb-4">Korpa je prazna</h2>
            <p className="text-gray-600 mb-8">Nije bilo predmeta u vašoj korpi</p>
            <a
              href="/"
              className="inline-block px-6 py-3 bg-black text-white rounded hover:bg-gray-800"
            >
              Nastavi kupovanje
            </a>
          </div>
        </div>
      </div>
    );
  }

  const subtotal = cart.items.reduce((sum, item) => sum + item.totalPrice, 0);
  const tax = subtotal * 0.2; // Example: 20% tax
  const total = subtotal + tax;

  return (
    <div className="min-h-screen bg-white">
      <div className="max-w-4xl mx-auto px-4 py-8">
        <h1 className="text-4xl font-bold mb-8">Korpa</h1>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-8">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Cart Items */}
          <div className="lg:col-span-2">
            <div className="space-y-4">
              {cart.items.map((item) => (
                <CartItem
                  key={item.id}
                  item={item}
                  onQuantityChange={handleUpdateQuantity}
                  onRemove={handleRemoveItem}
                />
              ))}
            </div>

            <div className="mt-8 flex gap-4">
              <a
                href="/"
                className="px-6 py-2 border border-black text-black rounded hover:bg-gray-100"
              >
                Nastavi kupovanje
              </a>
              <button
                onClick={handleClearCart}
                className="px-6 py-2 border border-red-600 text-red-600 rounded hover:bg-red-50"
              >
                Obriši korpu
              </button>
            </div>
          </div>

          {/* Order Summary */}
          <div className="lg:col-span-1">
            <div className="bg-gray-50 rounded-lg p-6 sticky top-4">
              <h2 className="text-2xl font-bold mb-6">Rezime Porudžbine</h2>

              <div className="space-y-4 mb-6 border-b pb-6">
                <div className="flex justify-between">
                  <span>Brendovi:</span>
                  <span className="font-semibold">
                    {new Set(cart.items.map((i) => i.brandName)).size}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span>Stavki:</span>
                  <span className="font-semibold">{cart.items.length}</span>
                </div>
                <div className="flex justify-between">
                  <span>Količina:</span>
                  <span className="font-semibold">
                    {cart.items.reduce((sum, i) => sum + i.quantity, 0)}
                  </span>
                </div>
              </div>

              <div className="space-y-3 mb-6 border-b pb-6">
                <div className="flex justify-between">
                  <span>Subtotal:</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Porezi:</span>
                  <span>{formatPrice(tax)}</span>
                </div>
              </div>

              <div className="flex justify-between text-xl font-bold mb-6">
                <span>Ukupno:</span>
                <span>{formatPrice(total)}</span>
              </div>

              <button className="w-full bg-black text-white py-3 rounded hover:bg-gray-800 font-semibold">
                Nastavi na Plaćanje
              </button>

              <p className="text-xs text-gray-600 text-center mt-4">
                Besplatna dostava za porudžbine preko 3000 din.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function CartItem({
  item,
  onQuantityChange,
  onRemove,
}: {
  item: CartItemDto;
  onQuantityChange: (itemId: string, quantity: number) => Promise<void>;
  onRemove: (itemId: string) => Promise<void>;
}) {
  const [updating, setUpdating] = useState(false);

  const handleQuantityChange = async (newQuantity: number) => {
    if (newQuantity < 1) return;
    setUpdating(true);
    try {
      await onQuantityChange(item.id, newQuantity);
    } finally {
      setUpdating(false);
    }
  };

  const handleRemove = async () => {
    setUpdating(true);
    try {
      await onRemove(item.id);
    } finally {
      setUpdating(false);
    }
  };

  return (
    <div className="border rounded-lg p-4 flex gap-4">
      {/* Product Image */}
      {item.productImageUrl && (
        <img
          src={item.productImageUrl}
          alt={item.productName}
          className="w-20 h-20 object-cover rounded"
        />
      )}

      {/* Product Info */}
      <div className="flex-1 min-w-0">
        <h3 className="font-semibold text-lg truncate">{item.productName}</h3>
        <p className="text-sm text-gray-600">{item.brandName}</p>
        {item.selectedSize && (
          <p className="text-sm text-gray-600">Veličina: {item.selectedSize}</p>
        )}
      </div>

      {/* Price & Quantity */}
      <div className="text-right">
        <div className="font-semibold mb-4">{formatPrice(item.unitPrice)}</div>

        <div className="flex items-center gap-2 mb-4">
          <button
            onClick={() => handleQuantityChange(item.quantity - 1)}
            disabled={updating || item.quantity <= 1}
            className="px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
          >
            −
          </button>
          <input
            type="number"
            value={item.quantity}
            onChange={(e) => handleQuantityChange(parseInt(e.target.value))}
            disabled={updating}
            className="w-12 text-center border rounded"
          />
          <button
            onClick={() => handleQuantityChange(item.quantity + 1)}
            disabled={updating}
            className="px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
          >
            +
          </button>
        </div>

        <div className="text-lg font-bold mb-4">{formatPrice(item.totalPrice)}</div>

        <button
          onClick={handleRemove}
          disabled={updating}
          className="text-red-600 hover:text-red-800 text-sm font-semibold disabled:opacity-50"
        >
          Ukloni
        </button>
      </div>
    </div>
  );
}
