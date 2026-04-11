'use client';

import { useEffect, useState, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import * as checkoutApi from '@/lib/api/checkout';
import { clearCartToken, getCartToken } from '@/lib/utils/cart-storage';
import { formatPrice } from '@/lib/utils/helpers';
import type {
  CheckoutRequest,
  CheckoutSummaryDto,
  DeliveryMethod,
  PaymentMethod,
} from '@/lib/types';

const initialForm: CheckoutRequest = {
  idempotencyKey: '',
  cartToken: '',
  customerFirstName: '',
  customerLastName: '',
  email: '',
  customerEmail: '',
  phone: '',
  deliveryAddressLine1: '',
  deliveryAddressLine2: '',
  deliveryCity: '',
  deliveryPostalCode: '',
  deliveryMethod: 'Courier' as DeliveryMethod,
  paymentMethod: 'CashOnDelivery' as PaymentMethod,
  note: '',
};

export default function CheckoutPage() {
  const router = useRouter();
  const [summary, setSummary] = useState<CheckoutSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<CheckoutRequest>(initialForm);

  useEffect(() => {
    const loadSummary = async () => {
      try {
        const token = getCartToken();
        if (!token) {
          router.push('/korpa');
          return;
        }

        const data = await checkoutApi.getCheckoutSummary(token);
        const idempotencyStorageKey = `checkout-idempotency:${token}`;
        const existingIdempotencyKey = window.sessionStorage.getItem(idempotencyStorageKey);
        const nextIdempotencyKey = existingIdempotencyKey || crypto.randomUUID();

        if (!existingIdempotencyKey) {
          window.sessionStorage.setItem(idempotencyStorageKey, nextIdempotencyKey);
        }

        setSummary(data);
        setForm((current) => ({
          ...current,
          cartToken: token,
          idempotencyKey: nextIdempotencyKey,
        }));
      } catch (err) {
        console.error(err);
        setError('Ne mogu da ucitam rezime korpe.');
      } finally {
        setLoading(false);
      }
    };

    void loadSummary();
  }, [router]);

  function handleInputChange<K extends keyof CheckoutRequest>(field: K, value: CheckoutRequest[K]) {
    setForm((current) => {
      const next = {
        ...current,
        [field]: value,
      };

      if (field === 'email') {
        next.customerEmail = String(value);
      }

      return next;
    });
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      if (!form.customerFirstName.trim()) throw new Error('Ime je obavezno.');
      if (!form.customerLastName.trim()) throw new Error('Prezime je obavezno.');
      if (!form.email.trim()) throw new Error('Email je obavezan.');
      if (!form.phone.trim()) throw new Error('Telefon je obavezan.');
      if (!form.deliveryAddressLine1.trim()) throw new Error('Adresa je obavezna.');
      if (!form.deliveryCity.trim()) throw new Error('Grad je obavezan.');
      if (!form.deliveryPostalCode.trim()) throw new Error('Postanski broj je obavezan.');

      const payload: CheckoutRequest = {
        ...form,
        customerEmail: form.email,
      };

      const result = await checkoutApi.placeOrder(payload);
      if (form.cartToken) {
        window.sessionStorage.removeItem(`checkout-idempotency:${form.cartToken}`);
      }
      clearCartToken();
      router.push(`/porudzbina/${result.orderNumber}`);
    } catch (err) {
      console.error(err);
      setError(err instanceof Error ? err.message : 'Ne mogu da zavrsim porudzbinu.');
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <p className="text-gray-600">Ucitavanje checkout-a...</p>
      </div>
    );
  }

  if (!summary) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Korpa je prazna</h1>
          <a href="/korpa" className="text-blue-600 hover:underline">
            Nazad na korpu
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto grid max-w-6xl grid-cols-1 gap-8 px-4 py-8 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <h1 className="mb-8 text-4xl font-bold">Checkout</h1>

          {error && (
            <div className="mb-8 rounded border border-red-300 bg-red-50 px-4 py-3 text-red-700">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-8">
            <div className="rounded-lg bg-white p-6">
              <h2 className="mb-6 text-2xl font-bold">Licni podaci</h2>
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <input
                  type="text"
                  placeholder="Ime"
                  value={form.customerFirstName}
                  onChange={(event) => handleInputChange('customerFirstName', event.target.value)}
                  className="rounded border px-4 py-2"
                  required
                />
                <input
                  type="text"
                  placeholder="Prezime"
                  value={form.customerLastName}
                  onChange={(event) => handleInputChange('customerLastName', event.target.value)}
                  className="rounded border px-4 py-2"
                  required
                />
                <input
                  type="email"
                  placeholder="Email"
                  value={form.email}
                  onChange={(event) => handleInputChange('email', event.target.value)}
                  className="rounded border px-4 py-2 md:col-span-2"
                  required
                />
                <input
                  type="tel"
                  placeholder="Telefon"
                  value={form.phone}
                  onChange={(event) => handleInputChange('phone', event.target.value)}
                  className="rounded border px-4 py-2 md:col-span-2"
                  required
                />
              </div>
            </div>

            <div className="rounded-lg bg-white p-6">
              <h2 className="mb-6 text-2xl font-bold">Adresa za isporuku</h2>
              <div className="space-y-4">
                <input
                  type="text"
                  placeholder="Adresa 1"
                  value={form.deliveryAddressLine1}
                  onChange={(event) => handleInputChange('deliveryAddressLine1', event.target.value)}
                  className="w-full rounded border px-4 py-2"
                  required
                />
                <input
                  type="text"
                  placeholder="Adresa 2 (opciono)"
                  value={form.deliveryAddressLine2 || ''}
                  onChange={(event) => handleInputChange('deliveryAddressLine2', event.target.value)}
                  className="w-full rounded border px-4 py-2"
                />
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <input
                    type="text"
                    placeholder="Grad"
                    value={form.deliveryCity}
                    onChange={(event) => handleInputChange('deliveryCity', event.target.value)}
                    className="rounded border px-4 py-2"
                    required
                  />
                  <input
                    type="text"
                    placeholder="Postanski broj"
                    value={form.deliveryPostalCode}
                    onChange={(event) => handleInputChange('deliveryPostalCode', event.target.value)}
                    className="rounded border px-4 py-2"
                    required
                  />
                </div>
              </div>
            </div>

            <div className="rounded-lg bg-white p-6">
              <h2 className="mb-6 text-2xl font-bold">Isporuka i placanje</h2>
              <div className="space-y-4">
                <div>
                  <label className="mb-2 block text-sm font-semibold">Nacin isporuke</label>
                  <select
                    value={form.deliveryMethod}
                    onChange={(event) => handleInputChange('deliveryMethod', event.target.value as DeliveryMethod)}
                    className="w-full rounded border px-4 py-2"
                  >
                    <option value="Courier">Kurir</option>
                    <option value="StorePickup">Preuzimanje u prodavnici</option>
                  </select>
                </div>

                <div>
                  <label className="mb-2 block text-sm font-semibold">Nacin placanja</label>
                  <select
                    value={form.paymentMethod}
                    onChange={(event) => handleInputChange('paymentMethod', event.target.value as PaymentMethod)}
                    className="w-full rounded border px-4 py-2"
                  >
                    <option value="CashOnDelivery">Placanje pri dostavi</option>
                    <option value="CardPlaceholder">Kartica</option>
                  </select>
                </div>

                <div>
                  <label className="mb-2 block text-sm font-semibold">Napomena</label>
                  <textarea
                    value={form.note || ''}
                    onChange={(event) => handleInputChange('note', event.target.value)}
                    className="w-full rounded border px-4 py-2"
                    rows={3}
                    placeholder="Dodatne napomene za porudzbinu"
                  />
                </div>
              </div>
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="w-full rounded bg-black py-3 font-semibold text-white hover:bg-gray-800 disabled:opacity-50"
            >
              {submitting ? 'Obrada porudzbine...' : 'Potvrdi porudzbinu'}
            </button>
          </form>
        </div>

        <aside className="lg:col-span-1">
          <div className="sticky top-4 rounded-lg bg-white p-6">
            <h2 className="mb-6 text-2xl font-bold">Rezime porudzbine</h2>

            <div className="mb-6 space-y-4 border-b pb-6">
              {summary.items.map((item, index) => (
                <div key={`${item.productName}-${index}`} className="text-sm">
                  <div className="flex justify-between">
                    <span>{item.productName}</span>
                    <span className="font-semibold">
                      {item.quantity}x {formatPrice(item.unitPrice)}
                    </span>
                  </div>
                  <div className="text-xs text-gray-600">
                    {item.brandName} • Velicina: {item.sizeEu}
                  </div>
                </div>
              ))}
            </div>

            <div className="mb-6 space-y-3">
              <div className="flex justify-between">
                <span>Proizvodi:</span>
                <span>{formatPrice(summary.subtotalAmount)}</span>
              </div>
              <div className="flex justify-between">
                <span>Isporuka:</span>
                <span>{formatPrice(summary.deliveryAmount)}</span>
              </div>
            </div>

            <div className="border-t pt-4">
              <div className="mb-6 flex justify-between text-xl font-bold">
                <span>Ukupno:</span>
                <span>{formatPrice(summary.totalAmount)}</span>
              </div>

              <div className="rounded bg-gray-50 p-4 text-sm text-gray-600">
                Bezbedna kupovina, brza isporuka i pravo na povracaj u skladu sa pravilima prodavnice.
              </div>
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
