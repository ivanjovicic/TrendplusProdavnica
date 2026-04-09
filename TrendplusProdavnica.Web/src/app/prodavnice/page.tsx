import { getStores } from '@/lib/api';
import { Breadcrumbs, EmptyState } from '@/components/common';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Naše Prodavnice',
  description: 'Pronađi našu prodavnicu blizu tebe',
};

export default async function StoresPage() {
  try {
    const stores = await getStores();

    const breadcrumbs = [
      { label: 'Početna', url: '/' },
      { label: 'Prodavnice', url: '/prodavnice' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold mb-4">Naše Prodavnice</h1>
          <p className="text-gray-600 text-lg mb-8">
            Poseti nas u gradovima gde smo dostupni
          </p>

          <Breadcrumbs items={breadcrumbs} />

          {/* Stores Grid */}
          <div className="mt-12">
            {stores.length === 0 ? (
              <EmptyState />
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {stores.map((store) => (
                  <a
                    key={store.id}
                    href={`/prodavnice/${store.slug}`}
                    className="group border rounded-lg p-6 hover:shadow-lg transition-shadow cursor-pointer"
                  >
                    {store.imageUrl && (
                      <img
                        src={store.imageUrl}
                        alt={store.name}
                        className="w-full h-48 object-cover rounded-lg mb-4 group-hover:scale-105 transition-transform"
                      />
                    )}
                    <h3 className="text-xl font-semibold mb-2 group-hover:text-gray-600 transition-colors">
                      {store.name}
                    </h3>
                    <div className="text-gray-600 text-sm space-y-1">
                      <p>{store.address}</p>
                      <p>{store.city}</p>
                      {store.phone && <p>Tel: {store.phone}</p>}
                      {store.email && <p>Email: {store.email}</p>}
                    </div>

                    {store.operatingHours && (
                      <div className="mt-4 pt-4 border-t">
                        <h4 className="text-sm font-semibold mb-2">Radno vreme:</h4>
                        <p className="text-xs text-gray-600">{store.operatingHours}</p>
                      </div>
                    )}
                  </a>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Greška pri učitavanju</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Nazad na početnu
          </a>
        </div>
      </div>
    );
  }
}
