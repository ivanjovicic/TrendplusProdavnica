import type { Metadata } from 'next';
import Link from 'next/link';
import Image from 'next/image';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { getBrandListing, getBrandPage } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 120;

interface BrandPageProps {
  params: Promise<{ slug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

function getRequestedPage(page?: string): number {
  const parsed = Number.parseInt(page || '1', 10);
  return Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

export async function generateMetadata({ params, searchParams }: BrandPageProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const brand = await getBrandPage(slug);

    return buildMetadata({
      title: brand.name,
      description: brand.shortDescription || brand.longDescription || brand.content || `Brend ${brand.name}.`,
      path: page > 1 ? `/brendovi/${slug}?page=${page}` : `/brendovi/${slug}`,
      seo: brand.seo,
      imageUrl: brand.coverImageUrl || brand.logoUrl,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Brend',
      description: 'Pregled brenda.',
      path: '/brendovi',
      type: 'website',
    });
  }
}

export default async function BrandPage({ params, searchParams }: BrandPageProps) {
  try {
    const { slug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const brand = await getBrandPage(slug);
    const listings = await getBrandListing(slug, {
      page,
      pageSize: 20,
    });

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Brendovi', url: '/brendovi' },
      { label: brand.name, url: `/brendovi/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          <div className="mb-12">
            {brand.logoUrl && (
              <Image
                src={brand.logoUrl}
                alt={brand.name}
                width={160}
                height={64}
                className="mb-6 h-16 w-auto object-contain"
                sizes="160px"
                priority
              />
            )}
            <h1 className="mb-4 text-4xl font-bold">{brand.name}</h1>
            <p className="max-w-2xl text-lg text-gray-600">
              {brand.longDescription || brand.shortDescription || brand.content}
            </p>
          </div>

          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            <h2 className="mb-8 text-2xl font-bold">Proizvodi</h2>
            {listings.products.length === 0 ? (
              <EmptyState />
            ) : (
              <>
                <ProductGrid products={listings.products} />

                {Math.ceil(listings.totalCount / listings.pageSize) > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from(
                      { length: Math.ceil(listings.totalCount / listings.pageSize) },
                      (_, index) => index + 1,
                    ).map((pageNumber) => (
                      <Link
                        key={pageNumber}
                        href={pageNumber === 1 ? `/brendovi/${slug}` : `/brendovi/${slug}?page=${pageNumber}`}
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
          <h1 className="mb-4 text-2xl font-bold">Brend nije pronadjen</h1>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}
