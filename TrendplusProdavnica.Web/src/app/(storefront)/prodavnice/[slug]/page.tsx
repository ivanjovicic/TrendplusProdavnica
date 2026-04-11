import type { Metadata } from 'next';
import Link from 'next/link';
import Image from 'next/image';
import { Breadcrumbs } from '@/components/common';
import { getStore } from '@/lib/api';
import { JsonLd, buildMetadata, buildStoreJsonLd } from '@/lib/seo';

export const revalidate = 300;

interface StoreDetailProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: StoreDetailProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const store = await getStore(slug);
    const description =
      store.shortDescription || store.description || `${store.address || store.addressLine1}, ${store.city}`;

    return buildMetadata({
      title: store.name,
      description,
      path: `/prodavnice/${slug}`,
      seo: store.seo,
      imageUrl: store.coverImageUrl || store.imageUrl,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Prodavnica',
      description: 'Detalj prodavnice.',
      path: '/prodavnice',
      type: 'website',
    });
  }
}

export default async function StoreDetailPage({ params }: StoreDetailProps) {
  try {
    const { slug } = await params;
    const store = await getStore(slug);
    const address = store.address || store.addressLine1;
    const description = store.shortDescription || store.description;
    const workingHours = store.workingHoursText || store.operatingHours;
    const imageUrl = store.coverImageUrl || store.imageUrl;

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Prodavnice', url: '/prodavnice' },
      { label: store.name, url: `/prodavnice/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-4xl px-4 py-8">
          <JsonLd data={buildStoreJsonLd(store)} />
          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            {imageUrl && (
              <div className="relative mb-8 h-96 w-full overflow-hidden rounded-lg">
                <Image
                  src={imageUrl}
                  alt={store.name}
                  fill
                  className="object-cover"
                  sizes="(max-width: 1024px) 100vw, 1024px"
                  priority
                />
              </div>
            )}

            <h1 className="mb-8 text-4xl font-bold">{store.name}</h1>

            <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
              <div>
                <h2 className="mb-4 text-2xl font-bold">Kontakt</h2>
                <div className="space-y-3 text-gray-700">
                  <div>
                    <strong>Adresa:</strong>
                    <p>{address}</p>
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

              <div>
                <h2 className="mb-4 text-2xl font-bold">Radno vreme</h2>
                {workingHours ? (
                  <div className="whitespace-pre-line text-gray-700">{workingHours}</div>
                ) : (
                  <p className="text-gray-500">Nema dostupnog radnog vremena.</p>
                )}
              </div>
            </div>

            {description && (
              <div className="mt-12">
                <h2 className="mb-4 text-2xl font-bold">O prodavnici</h2>
                <p className="leading-relaxed text-gray-700">{description}</p>
              </div>
            )}

            <div className="mt-12">
              <Link href="/prodavnice" className="inline-block text-blue-600 hover:underline">
                Nazad na sve prodavnice
              </Link>
            </div>
          </div>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Prodavnica nije pronadjena</h1>
          <Link href="/prodavnice" className="text-blue-600 hover:underline">
            Nazad na prodavnice
          </Link>
        </div>
      </div>
    );
  }
}
