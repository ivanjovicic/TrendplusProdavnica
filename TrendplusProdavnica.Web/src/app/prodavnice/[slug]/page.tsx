import { getStore } from '@/lib/api';
import { Breadcrumbs } from '@/components/common';
import { Metadata } from 'next';

interface StoreDetailProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: StoreDetailProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const store = await getStore(slug);
    return {
      title: store.name,
      description: `${store.address}, ${store.city}`,
    };
  } catch (error) {
    return {
      title: 'Store Not Found',
    };
  }
}

export default async function StoreDetailPage({ params }: StoreDetailProps) {
  try {
    const { slug } = await params;
    const store = await getStore(slug);

    const breadcrumbs = [
      { label: 'Početna', slug: '', url: '/' },
      { label: 'Prodavnice', slug: 'prodavnice', url: '/prodavnice' },
      { label: store.name, slug: slug, url: `/prodavnice/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-4xl mx-auto px-4 py-8">
          <Breadcrumbs items={breadcrumbs} />

          {/* Store Hero */}
          <div className="mt-12">
            {store.imageUrl && (
              <img
                src={store.imageUrl}
                alt={store.name}
                className="w-full h-96 object-cover rounded-lg mb-8"
              />
            )}

            <h1 className="text-4xl font-bold mb-8">{store.name}</h1>

            {/* Store Info */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {/* Contact Info */}
              <div>
                <h2 className="text-2xl font-bold mb-4">Kontakt</h2>
                <div className="space-y-3 text-gray-700">
                  <div>
                    <strong>Adresa:</strong>
                    <p>{store.address}</p>
                    <p>{store.city}</p>
                  </div>

                  {store.phone && (
                    <div>
                      <strong>Telefon:</strong>
                      <p>
                        <a href={`tel:${store.phone}`} className="text-blue-600 hover:underline">
                          {store.phone}
                        </a>
                      </p>
                    </div>
                  )}

                  {store.email && (
                    <div>
                      <strong>Email:</strong>
                      <p>
                        <a href={`mailto:${store.email}`} className="text-blue-600 hover:underline">
                          {store.email}
                        </a>
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {/* Opening Hours */}
              <div>
                <h2 className="text-2xl font-bold mb-4">Radno Vreme</h2>
                {store.operatingHours ? (
                  <div className="text-gray-700 whitespace-pre-line">
                    {store.operatingHours}
                  </div>
                ) : (
                  <p className="text-gray-500">Nema dostupnog radnog vremena</p>
                )}
              </div>
            </div>

            {/* Description */}
            {store.description && (
              <div className="mt-12">
                <h2 className="text-2xl font-bold mb-4">O Prodavnici</h2>
                <p className="text-gray-700 leading-relaxed">
                  {store.description}
                </p>
              </div>
            )}

            {/* Amenities */}
            {store.amenities && store.amenities.length > 0 && (
              <div className="mt-12">
                <h2 className="text-2xl font-bold mb-4">Mogućnosti</h2>
                <ul className="space-y-2">
                  {store.amenities.map((amenity, i) => (
                    <li key={i} className="flex items-center gap-2">
                      <span className="text-green-600 font-bold">✓</span>
                      <span>{amenity}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Back Link */}
            <div className="mt-12">
              <a
                href="/prodavnice"
                className="inline-block text-blue-600 hover:underline"
              >
                ← Nazad na sve prodavnice
              </a>
            </div>
          </div>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Prodavnica nije pronađena</h1>
          <a href="/prodavnice" className="text-blue-600 hover:underline">
            Nazad na prodavnice
          </a>
        </div>
      </div>
    );
  }
}
