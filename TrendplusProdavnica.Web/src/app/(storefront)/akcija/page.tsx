import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { getSaleListing } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 120;

interface SalePageProps {
  searchParams?: Promise<{ page?: string; sort?: string }>;
}

function getRequestedPage(page?: string): number {
  const parsed = Number.parseInt(page || '1', 10);
  return Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

export async function generateMetadata({ searchParams }: SalePageProps): Promise<Metadata> {
  const resolvedSearchParams = (await searchParams) || {};
  const page = getRequestedPage(resolvedSearchParams.page);

  return buildMetadata({
    title: 'Akcija',
    description: 'Pregled aktuelno snizenih proizvoda u Trendplus ponudi.',
    path: page > 1 ? `/akcija?page=${page}` : '/akcija',
    type: 'website',
  });
}

export default async function SalePage({ searchParams }: SalePageProps) {
  try {
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const sort = resolvedSearchParams.sort || 'newest';
    const listing = await getSaleListing({
      page,
      pageSize: 20,
      sort,
    });
    const pageCount = Math.ceil(listing.totalCount / listing.pageSize);
    const maxDiscount = listing.products.reduce((currentMax, product) => {
      if (!product.oldPrice || product.oldPrice <= product.price) {
        return currentMax;
      }

      const discount = Math.round(((product.oldPrice - product.price) / product.oldPrice) * 100);
      return Math.max(currentMax, discount);
    }, 0);

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Akcija', url: '/akcija' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          <h1 className="mb-2 text-4xl font-bold">AKCIJA</h1>
          <p className="mb-8 text-lg font-semibold text-red-600">
            {maxDiscount > 0 ? `Snizeni proizvodi do ${maxDiscount}% OFF` : 'Aktuelni snizeni proizvodi'}
          </p>

          <Breadcrumbs items={breadcrumbs} />

          <div className="my-8 flex flex-wrap gap-4">
            <Link
              href="/akcija?sort=newest"
              className={`rounded border px-4 py-2 ${
                sort === 'newest' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Najnovije
            </Link>
            <Link
              href="/akcija?sort=price-asc"
              className={`rounded border px-4 py-2 ${
                sort === 'price-asc' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Cena: niska ka visokoj
            </Link>
            <Link
              href="/akcija?sort=price-desc"
              className={`rounded border px-4 py-2 ${
                sort === 'price-desc' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Cena: visoka ka niskoj
            </Link>
          </div>

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
                        href={pageNumber === 1 ? `/akcija?sort=${sort}` : `/akcija?page=${pageNumber}&sort=${sort}`}
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
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Greska pri ucitavanju akcije';

    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Greska pri ucitavanju</h1>
          <p className="mb-4 text-sm text-gray-600">{message}</p>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}
