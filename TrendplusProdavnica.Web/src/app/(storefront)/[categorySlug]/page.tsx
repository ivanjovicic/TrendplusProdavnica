import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs, Container, EmptyState, ProductGrid } from '@/components';
import { getCategoryListing } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 120;

interface PageProps {
  params: Promise<{ categorySlug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

function getRequestedPage(page?: string): number {
  const parsed = Number.parseInt(page || '1', 10);
  return Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;
}

export async function generateMetadata({ params, searchParams }: PageProps): Promise<Metadata> {
  try {
    const { categorySlug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const listing = await getCategoryListing(categorySlug, { page, pageSize: 24 });

    return buildMetadata({
      title: listing.seo?.seoTitle || listing.title,
      description: listing.seo?.seoDescription || listing.intro || `Pregled kategorije ${listing.title}.`,
      path: page > 1 ? `/${categorySlug}?page=${page}` : `/${categorySlug}`,
      seo: listing.seo,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Kategorija',
      description: 'Pregled kategorije proizvoda.',
      path: '/',
      type: 'website',
    });
  }
}

export default async function CategoryPage({ params, searchParams }: PageProps) {
  try {
    const { categorySlug } = await params;
    const resolvedSearchParams = (await searchParams) || {};
    const page = getRequestedPage(resolvedSearchParams.page);
    const data = await getCategoryListing(categorySlug, { page, pageSize: 24 });
    const pageCount = Math.ceil(data.totalCount / data.pageSize);

    return (
      <div>
        <Container>
          <Breadcrumbs items={data.breadcrumbs} />
          <h1 className="mb-2 text-4xl font-bold">{data.title}</h1>
          {data.intro && <p className="mb-8 text-lg text-gray-600">{data.intro}</p>}

          {data.products.length > 0 ? (
            <>
              <ProductGrid products={data.products} />

              {pageCount > 1 && (
                <div className="mt-8 flex flex-wrap justify-center gap-2">
                  {Array.from({ length: pageCount }, (_, index) => index + 1).map((pageNumber) => (
                    <Link
                      key={pageNumber}
                      href={pageNumber === 1 ? `/${categorySlug}` : `/${categorySlug}?page=${pageNumber}`}
                      className={`rounded border px-3 py-1 ${
                        pageNumber === page ? 'bg-black text-white' : 'hover:bg-gray-100'
                      }`}
                    >
                      {pageNumber}
                    </Link>
                  ))}
                </div>
              )}
            </>
          ) : (
            <EmptyState />
          )}
        </Container>
      </div>
    );
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Greska pri ucitavanju kategorije';
    return (
      <Container>
        <div className="py-12 text-red-600">Greska: {message}</div>
      </Container>
    );
  }
}
