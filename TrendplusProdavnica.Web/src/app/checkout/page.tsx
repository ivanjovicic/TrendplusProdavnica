'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import * as checkoutApi from '@/lib/api/checkout';
import * as cartApi from '@/lib/api/cart';
import { getCartToken, clearCartToken } from '@/lib/utils/cart-storage';
import { formatPrice } from '@/lib/utils/helpers';
import { CheckoutSummaryDto, CheckoutRequest } from '@/lib/types';

export default function CheckoutPage() {
  const router = useRouter();
  const [summary, setSummary] = useState<CheckoutSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Form state
  const [form, setForm] = useState<CheckoutRequest>({
    cartToken: '',
    customerFirstName: '',
    customerLastName: '',
    email: '',
    phone: '',
    deliveryAddressLine1: '',
    deliveryAddressLine2: '',
    deliveryCity: '',
    deliveryPostalCode: '',
    deliveryMethod: 'Courier' as DeliveryMethod,
    paymentMethod: 'CashOnDelivery' as PaymentMethod,
    note: '',
  });

  // Load cart summary on mount
  useEffect(() => {
    const loadSummary = async () => {
      try {
        const token = getCartToken();
        if (!token) {
          router.push('/');
          return;
        }

        const data = await checkoutApi.getCheckoutSummary(token);
        if (!data) {
          setError('Cart is empty');
          return;
        }

        setSummary(data);
        setForm((prev) => ({
          ...prev,
          cartToken: token,
        }));
      } catch (err) {
        setError('Failed to load cart summary');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadSummary();
  }, [router]);

  const handleInputChange = (field: string, value: string) => {
    setForm((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      // Validate form
      if (!form.customerFirstName.trim()) throw new Error('First name is required');
      if (!form.customerLastName.trim()) throw new Error('Last name is required');
      if (!form.email.trim()) throw new Error('Email is required');
      if (!form.phone.trim()) throw new Error('Phone is required');
      if (!form.deliveryAddressLine1.trim()) throw new Error('Address is required');
      if (!form.deliveryCity.trim()) throw new Error('City is required');
      if (!form.deliveryPostalCode.trim()) throw new Error('Postal code is required');

      // Place order
      const result = await checkoutApi.placeOrder(form);

      // Clear cart token
      clearCartToken();

      // Redirect to order confirmation
      router.push(`/porudzbina/${result.orderNumber}`);
    } catch (err: any) {
      setError(err.message || 'Failed to place order');
      console.error(err);
    } finally {
      setSubmitting(false);
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

  if (!summary) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Корпа је празна</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Назад на почетну
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-6xl mx-auto px-4 py-8">
        <h1 className="text-4xl font-bold mb-8">Плаћање</h1>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-8">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Checkout Form */}
          <div className="lg:col-span-2">
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Customer Info */}
              <div className="bg-white rounded-lg p-6">
                <h2 className="text-2xl font-bold mb-6">Лични подаци</h2>
                <div className="grid grid-cols-2 gap-4">
                  <input
                    type="text"
                    placeholder="Име"
                    value={form.customerFirstName}
                    onChange={(e) =>
                      handleInputChange('customerFirstName', e.target.value)
                    }
                    className="col-span-1 px-4 py-2 border rounded"
                    required
                  />
                  <input
                    type="text"
                    placeholder="Презиме"
                    value={form.customerLastName}
                    onChange={(e) =>
                      handleInputChange('customerLastName', e.target.value)
                    }
                    className="col-span-1 px-4 py-2 border rounded"
                    required
                  />
                  <input
                    type="email"
                    placeholder="Email"
                    value={form.email}
                    onChange={(e) => handleInputChange('email', e.target.value)}
                    className="col-span-2 px-4 py-2 border rounded"
                    required
                  />
                  <input
                    type="tel"
                    placeholder="Телефон"
                    value={form.phone}
                    onChange={(e) => handleInputChange('phone', e.target.value)}
                    className="col-span-2 px-4 py-2 border rounded"
                    required
                  />
                </div>
              </div>

              {/* Delivery Address */}
              <div className="bg-white rounded-lg p-6">
                <h2 className="text-2xl font-bold mb-6">Адреса за испоруку</h2>
                <div className="space-y-4">
                  <input
                    type="text"
                    placeholder="Адреса 1"
                    value={form.deliveryAddressLine1}
                    onChange={(e) =>
                      handleInputChange('deliveryAddressLine1', e.target.value)
                    }
                    className="w-full px-4 py-2 border rounded"
                    required
                  />
                  <input
                    type="text"
                    placeholder="Адреса 2 (опционо)"
                    value={form.deliveryAddressLine2 || ''}
                    onChange={(e) =>
                      handleInputChange('deliveryAddressLine2', e.target.value)
                    }
                    className="w-full px-4 py-2 border rounded"
                  />
                  <div className="grid grid-cols-2 gap-4">
                    <input
                      type="text"
                      placeholder="Град"
                      value={form.deliveryCity}
                      onChange={(e) =>
                        handleInputChange('deliveryCity', e.target.value)
                      }
                      className="col-span-1 px-4 py-2 border rounded"
                      required
                    />
                    <input
                      type="text"
                      placeholder="Поштански број"
                      value={form.deliveryPostalCode}
                      onChange={(e) =>
                        handleInputChange('deliveryPostalCode', e.target.value)
                      }
                      className="col-span-1 px-4 py-2 border rounded"
                      required
                    />
                  </div>
                </div>
              </div>

              {/* Delivery & Payment */}
              <div className="bg-white rounded-lg p-6">
                <h2 className="text-2xl font-bold mb-6">Испорука и плаћање</h2>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-semibold mb-2">
                      Начин испоруке
                    </label>
                    <select
                      value={form.deliveryMethod}
                      onChange={(e) =>
                        handleInputChange('deliveryMethod', e.target.value)
                      }
                      className="w-full px-4 py-2 border rounded"
                    >
                      <option value="Courier">Курир (300 дин.)</option>
                      <option value="StorePickup">Преузимање у продавници (0 дин.)</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold mb-2">
                      Начин плаћања
                    </label>
                    <select
                      value={form.paymentMethod}
                      onChange={(e) =>
                        handleInputChange('paymentMethod', e.target.value)
                      }
                      className="w-full px-4 py-2 border rounded"
                    >
                      <option value="CashOnDelivery">Плаћање при доставци</option>
                      <option value="CardPlaceholder">Картица (убрзо)</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold mb-2">
                      Напомена (опционо)
                    </label>
                    <textarea
                      value={form.note || ''}
                      onChange={(e) => handleInputChange('note', e.target.value)}
                      className="w-full px-4 py-2 border rounded"
                      rows={3}
                      placeholder="Додатни напомене за вашу поредбину..."
                    />
                  </div>
                </div>
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-black text-white py-3 rounded font-semibold hover:bg-gray-800 disabled:opacity-50"
              >
                {submitting ? 'Плаћање у току...' : 'Потврди поредбину'}
              </button>
            </form>
          </div>

          {/* Order Summary Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg p-6 sticky top-4">
              <h2 className="text-2xl font-bold mb-6">Резиме поредбине</h2>

              {/* Items */}
              <div className="space-y-4 mb-6 border-b pb-6">
                {summary.items.map((item, i) => (
                  <div key={i} className="text-sm">
                    <div className="flex justify-between">
                      <span>{item.productName}</span>
                      <span className="font-semibold">
                        {item.quantity}x {formatPrice(item.unitPrice)}
                      </span>
                    </div>
                    <div className="text-gray-600 text-xs">
                      {item.brandName} • Величина: {item.sizeEu}
                    </div>
                  </div>
                ))}
              </div>

              {/* Totals */}
              <div className="space-y-3 mb-6">
                <div className="flex justify-between">
                  <span>Укупно производе:</span>
                  <span>{formatPrice(summary.subtotalAmount)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Испорука:</span>
                  <span>{formatPrice(summary.deliveryAmount)}</span>
                </div>
              </div>

              {/* Total */}
              <div className="border-t pt-4">
                <div className="flex justify-between text-xl font-bold mb-6">
                  <span>Укупна цена:</span>
                  <span>{formatPrice(summary.totalAmount)}</span>
                </div>

                {/* Info Box */}
                <div className="bg-gray-50 p-4 rounded text-sm text-gray-600">
                  <p className="mb-2">
                    ✓ Безбедна куповина<br/>
                    ✓ Брза испорука<br/>
                    ✓ Право на враћање
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
