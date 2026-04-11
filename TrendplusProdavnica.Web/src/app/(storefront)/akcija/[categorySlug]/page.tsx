import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { getSaleCategoryListing } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 120;

interface SaleCategoryPageProps {
  params: Promise<{ categorySlug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

function getRequestedPage(page?: string): number {
  const parsed = Number.parseInt(page || '1', 10);
  return Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

export async function generateMetadata({
  params,
  searchParams,
}: SaleCategoryPageProps): Promise<Metadata> {
  try {
    const { categorySlug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const listing = await getSaleCategoryListing(categorySlug, { page, pageSize: 20 });

    return buildMetadata({
      title: `Akcija - ${listing.title}`,
      description: listing.seo?.seoDescription || listing.intro || `Snizeni proizvodi u kategoriji ${listing.title}.`,
      path: page > 1 ? `/akcija/${categorySlug}?page=${page}` : `/akcija/${categorySlug}`,
      seo: listing.seo,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Akcija',
      description: 'Snizeni proizvodi po kategorijama.',
      path: '/akcija',
      type: 'website',
    });
  }
}

export default async function SaleCategoryPage({
  params,
  searchParams,
}: SaleCategoryPageProps) {
  try {
    const { categorySlug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const listing = await getSaleCategoryListing(categorySlug, {
      page,
      pageSize: 20,
    });
    const pageCount = Math.ceil(listing.totalCount / listing.pageSize);

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Akcija', url: '/akcija' },
      { label: listing.title, url: `/akcija/${categorySlug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          <h1 className="mb-2 text-4xl font-bold">AKCIJA - {listing.title.toUpperCase()}</h1>
          <p className="mb-8 text-lg text-gray-600">Snizeni proizvodi u ovoj kategoriji.</p>

          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            {listing.products.length === 0 ? (
              <EmptyState />
            ) : (
              <>
                <ProductGrid products={listing.products} />

                {pageCount > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from({ length: pageCount }, (_, index) => index + 1).map((pageNumber) => (
                      <Link
                        key={pageNumber}
                        href={pageNumber === 1 ? `/akcija/${categorySlug}` : `/akcija/${categorySlug}?page=${pageNumber}`}
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

          <div className="mt-12 flex gap-4">
            <Link href="/akcija" className="text-blue-600 hover:underline">
              Nazad na sve akcije
            </Link>
            <Link href="/" className="text-blue-600 hover:underline">
              Nazad na pocetnu
            </Link>
          </div>
        </div>
      </div>
    );
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Greska pri ucitavanju akcijske kategorije';

    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Kategorija nije pronadjena</h1>
          <p className="mb-4 text-sm text-gray-600">{message}</p>
          <Link href="/akcija" className="text-blue-600 hover:underline">
            Nazad na sve akcije
          </Link>
        </div>
      </div>
    );
  }
}
