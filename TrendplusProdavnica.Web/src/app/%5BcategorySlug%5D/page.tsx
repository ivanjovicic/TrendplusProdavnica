'use client';

import { use, useState, useCallback } from 'react';
import { Container, ProductGrid, Breadcrumbs, Pagination, EmptyState } from '@/components';
import { getCategoryListing } from '@/lib/api';
import type { ProductListingPageDto } from '@/lib/types';

interface PageProps {
  params: Promise<{ categorySlug: string }>;
  searchParams: Promise<{ [key: string]: string | string[] | undefined }>;
}

export default function CategoryPage({ params: paramsPromise, searchParams }: PageProps) {
  const params = use(paramsPromise);
  const [data, setData] = useState<ProductListingPageDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  use(
    (async () => {
      try {
        setLoading(true);
        const result = await getCategoryListing(params.categorySlug, { page, pageSize: 24 });
        setData(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Error loading category');
      } finally {
        setLoading(false);
      }
    })()
  );

  if (error) return <Container><div className="text-red-600 py-12">Greška: {error}</div></Container>;

  return (
    <div>
      <Container>
        {data && (
          <>
            <Breadcrumbs items={data.breadcrumbs} />
            <h1 className="text-4xl font-bold mb-2">{data.title}</h1>
            {data.intro && <p className="text-gray-600 mb-8 text-lg">{data.intro}</p>}
            
            {loading ? (
              <div className="animate-pulse space-y-4">
                {Array.from({ length: 8 }).map((_, i) => (
                  <div key={i} className="h-32 bg-gray-200 rounded" />
                ))}
              </div>
            ) : data.products.length > 0 ? (
              <>
                <ProductGrid products={data.products} />
                <Pagination page={page} pageSize={24} totalCount={data.totalCount} onPageChange={setPage} />
              </>
            ) : (
              <EmptyState />
            )}
          </>
        )}
      </Container>
    </div>
  );
}
