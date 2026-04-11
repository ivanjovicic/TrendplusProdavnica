import type { Metadata } from 'next';
import Link from 'next/link';
import Image from 'next/image';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { getCollectionListing, getCollectionPage } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 120;

interface CollectionPageProps {
  params: Promise<{ slug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

function getRequestedPage(page?: string): number {
  const parsed = Number.parseInt(page || '1', 10);
  return Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

export async function generateMetadata({ params, searchParams }: CollectionPageProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const collection = await getCollectionPage(slug);

    return buildMetadata({
      title: collection.name,
      description: collection.longDescription || collection.shortDescription || `Kolekcija ${collection.name}.`,
      path: page > 1 ? `/kolekcije/${slug}?page=${page}` : `/kolekcije/${slug}`,
      seo: collection.seo,
      imageUrl: collection.coverImageUrl || collection.thumbnailImageUrl,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Kolekcija',
      description: 'Pregled kolekcije.',
      path: '/kolekcije',
      type: 'website',
    });
  }
}

export default async function CollectionPage({ params, searchParams }: CollectionPageProps) {
  const { slug } = await params;
  const resolvedSearchParams = (await searchParams) || {};

  try {
    const collection = await getCollectionPage(slug);
    const page = getRequestedPage(resolvedSearchParams.page);
    const listings = await getCollectionListing(slug, {
      page,
      pageSize: 20,
    });

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Kolekcije', url: '/kolekcije' },
      { label: collection.name, url: `/kolekcije/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          {collection.coverImageUrl && (
            <div className="relative mb-8 h-96 w-full overflow-hidden rounded-lg">
              <Image
                src={collection.coverImageUrl}
                alt={collection.name}
                fill
                className="object-cover"
                sizes="(max-width: 1280px) 100vw, 1280px"
                priority
              />
            </div>
          )}

          <h1 className="mb-4 text-4xl font-bold">{collection.name}</h1>
          <p className="mb-8 max-w-2xl text-lg text-gray-600">
            {collection.longDescription || collection.shortDescription}
          </p>

          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            {listings.products.length === 0 ? (
              <EmptyState />
            ) : (
              <>
                <h2 className="mb-8 text-2xl font-bold">Proizvodi</h2>
                <ProductGrid products={listings.products} />

                {Math.ceil(listings.totalCount / listings.pageSize) > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from(
                      { length: Math.ceil(listings.totalCount / listings.pageSize) },
                      (_, index) => index + 1,
                    ).map((pageNumber) => (
                      <Link
                        key={pageNumber}
                        href={pageNumber === 1 ? `/kolekcije/${slug}` : `/kolekcije/${slug}?page=${pageNumber}`}
                        className={`rounded border px-4 py-2 ${
                          pageNumber === page ? 'bg-black text-white' : 'bg-white text-black hover:bg-gray-100'
                        }`}
                      >
                        {pageNumber}
                      </Link>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Kolekcija nije pronadjena</h1>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}
