import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs, EmptyState } from '@/components/common';
import { getStores } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const metadata: Metadata = buildMetadata({
  title: 'Prodavnice',
  description: 'Pronadji Trendplus prodavnice i planiraj posetu.',
  path: '/prodavnice',
  type: 'website',
});

export default async function StoresPage() {
  try {
    const storesResponse = await getStores();
    const stores = storesResponse.items;
    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Prodavnice', url: '/prodavnice' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          <h1 className="mb-4 text-4xl font-bold">Nase prodavnice</h1>
          <p className="mb-8 text-lg text-gray-600">Poseti nas u gradovima gde smo dostupni.</p>

          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            {stores.length === 0 ? (
              <EmptyState />
            ) : (
              <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                {stores.map((store) => {
                  const imageUrl = store.coverImageUrl || store.imageUrl;
                  const address = store.address || store.addressLine1;
                  const workingHours = store.workingHoursText || store.operatingHours;

                  return (
                    <Link
                      key={store.id}
                      href={`/prodavnice/${store.slug}`}
                      className="group cursor-pointer rounded-lg border p-6 transition-shadow hover:shadow-lg"
                    >
                      {imageUrl && (
                        <img
                          src={imageUrl}
                          alt={store.name}
                          className="mb-4 h-48 w-full rounded-lg object-cover transition-transform group-hover:scale-105"
                        />
                      )}
                      <h3 className="mb-2 text-xl font-semibold transition-colors group-hover:text-gray-600">
                        {store.name}
                      </h3>
                      <div className="space-y-1 text-sm text-gray-600">
                        <p>{address}</p>
                        <p>{store.city}</p>
                        {store.phone && <p>Tel: {store.phone}</p>}
                        {store.email && <p>Email: {store.email}</p>}
                      </div>

                      {workingHours && (
                        <div className="mt-4 border-t pt-4">
                          <h4 className="mb-2 text-sm font-semibold">Radno vreme:</h4>
                          <p className="text-xs text-gray-600">{workingHours}</p>
                        </div>
                      )}
                    </Link>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Greska pri ucitavanju</h1>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}
