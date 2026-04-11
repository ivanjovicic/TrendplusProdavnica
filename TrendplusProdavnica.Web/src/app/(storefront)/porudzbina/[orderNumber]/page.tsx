import type { Metadata } from 'next';
import Link from 'next/link';
import { getOrder } from '@/lib/api/checkout';
import { formatPrice } from '@/lib/utils/helpers';

interface OrderPageProps {
  params: Promise<{ orderNumber: string }>;
}

export async function generateMetadata({ params }: OrderPageProps): Promise<Metadata> {
  const { orderNumber } = await params;

  return {
    title: `Porudzbina ${orderNumber}`,
    description: 'Detalji vase porudzbine.',
  };
}

export default async function OrderConfirmationPage({ params }: OrderPageProps) {
  try {
    const { orderNumber } = await params;
    const order = await getOrder(orderNumber);

    return (
      <div className="min-h-screen bg-gray-50">
        <div className="mx-auto max-w-4xl px-4 py-8">
          <div className="mb-8 rounded-lg border border-green-200 bg-green-50 p-8 text-center">
            <div className="mb-4 text-5xl">OK</div>
            <h1 className="mb-2 text-3xl font-bold text-green-800">Porudzbina je potvrdjena</h1>
            <p className="mb-2 text-gray-700">
              Hvala na kupovini. Broj porudzbine: <strong>{order.orderNumber}</strong>
            </p>
            <p className="text-sm text-gray-600">Potvrda je poslata na email adresu koju ste uneli.</p>
          </div>

          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            <div className="space-y-6 lg:col-span-2">
              <div className="rounded-lg bg-white p-6">
                <h2 className="mb-6 text-2xl font-bold">Detalji porudzbine</h2>

                <div className="mb-6 space-y-3 border-b pb-6">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Broj porudzbine:</span>
                    <span className="font-semibold">{order.orderNumber}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Status:</span>
                    <span className="font-semibold">{getPrettyStatus(order.status)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Datum:</span>
                    <span className="font-semibold">{new Date(order.createdAt).toLocaleDateString('sr-RS')}</span>
                  </div>
                </div>

                <h3 className="mb-3 font-bold">Licni podaci</h3>
                <div className="mb-6 space-y-1 border-b pb-6 text-sm text-gray-700">
                  <p>{order.customerFullName}</p>
                  <p>{order.email}</p>
                  <p>{order.phone}</p>
                </div>

                <h3 className="mb-3 font-bold">Adresa za isporuku</h3>
                <div className="mb-6 space-y-1 border-b pb-6 text-sm text-gray-700">
                  <p>{order.deliveryAddressLine1}</p>
                  {order.deliveryAddressLine2 && <p>{order.deliveryAddressLine2}</p>}
                  <p>
                    {order.deliveryCity}, {order.deliveryPostalCode}
                  </p>
                </div>

                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <div>
                    <p className="mb-1 text-sm text-gray-600">Isporuka</p>
                    <p className="font-semibold">{getPrettyDeliveryMethod(order.deliveryMethod)}</p>
                  </div>
                  <div>
                    <p className="mb-1 text-sm text-gray-600">Placanje</p>
                    <p className="font-semibold">{getPrettyPaymentMethod(order.paymentMethod)}</p>
                  </div>
                </div>
              </div>

              <div className="rounded-lg bg-white p-6">
                <h2 className="mb-6 text-2xl font-bold">Proizvodi</h2>

                <div className="space-y-4">
                  {order.items.map((item, index) => (
                    <div key={`${item.productName}-${index}`} className="flex items-center justify-between border-b pb-4">
                      <div>
                        <p className="font-semibold">{item.productName}</p>
                        <p className="text-sm text-gray-600">
                          {item.brandName} • Velicina: {item.sizeEu}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold">
                          {item.quantity}x {formatPrice(item.unitPrice)}
                        </p>
                        <p className="text-sm text-gray-600">Ukupno: {formatPrice(item.lineTotal)}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            <div className="lg:col-span-1">
              <div className="sticky top-4 rounded-lg bg-white p-6">
                <h2 className="mb-6 text-2xl font-bold">Rezime placanja</h2>

                <div className="mb-6 space-y-3 border-b pb-6">
                  <div className="flex justify-between">
                    <span>Proizvodi:</span>
                    <span>{formatPrice(order.subtotalAmount)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Isporuka:</span>
                    <span>{formatPrice(order.deliveryAmount)}</span>
                  </div>
                </div>

                <div className="mb-8 flex justify-between text-xl font-bold">
                  <span>Ukupno:</span>
                  <span>{formatPrice(order.totalAmount)}</span>
                </div>

                <div className="mb-6 rounded border border-blue-200 bg-blue-50 p-4 text-sm text-blue-800">
                  <p className="mb-2 font-semibold">Sta sledi dalje?</p>
                  <ul className="space-y-1 text-xs">
                    <li>Potvrda email-om</li>
                    <li>Priprema pakovanja</li>
                    <li>Brza isporuka</li>
                    <li>Pracenje posiljke</li>
                  </ul>
                </div>

                <Link
                  href="/"
                  className="block w-full rounded bg-black py-2 text-center font-semibold text-white hover:bg-gray-800"
                >
                  Nazad na pocetnu
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Greska pri ucitavanju porudzbine</h1>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}

function getPrettyStatus(status: string) {
  const statuses: Record<string, string> = {
    Pending: 'Na cekanju',
    Paid: 'Placeno',
    Shipped: 'Poslato',
    Completed: 'Zavrseno',
    Cancelled: 'Otkazano',
  };

  return statuses[status] || status;
}

function getPrettyDeliveryMethod(method: string) {
  const methods: Record<string, string> = {
    Courier: 'Kurir',
    StorePickup: 'Preuzimanje u prodavnici',
  };

  return methods[method] || method;
}

function getPrettyPaymentMethod(method: string) {
  const methods: Record<string, string> = {
    CashOnDelivery: 'Placanje pri dostavi',
    CardPlaceholder: 'Kartica',
  };

  return methods[method] || method;
}
