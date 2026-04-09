import { getSaleCategoryListing } from '@/lib/api';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { Metadata } from 'next';
  params: Promise<{ categorySlug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

export async function generateMetadata({ params }: SaleCategoryPageProps): Promise<Metadata> {
  const { categorySlug } = await params;
  return {
    title: `Akcija - ${categorySlug}`,
    description: `Sniženi proizvodi u kategoriji ${categorySlug}`,
  };
}

export default async function SaleCategoryPage({
  params,
  searchParams,
}: SaleCategoryPageProps) {
  try {
    const { categorySlug } = await params;
    const resolvedsearchParams = searchParams ? await searchParams : {};
    const page = parseInt(resolvedsearchParams?.page || '1');

    const listings = await getSaleCategoryListing(categorySlug, {
      page,
      pageSize: 20,
    });

    const breadcrumbs = [
      { label: 'Početna', url: '/' },
      { label: 'Akcija', url: '/akcija' },
      { label: categorySlug, url: `/akcija/${categorySlug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold mb-2">
            AKCIJA - {params.categorySlug.toUpperCase()}
          </h1>
          <p className="text-gray-600 text-lg mb-8">
            Sniženi proizvodi u ovoj kategoriji
          </p>

          <Breadcrumbs items={breadcrumbs} />

          {/* Products */}
          <div className="mt-12">
            {listings.items.length === 0 ? (
              <EmptyState />
            ) : (
              <>
                <ProductGrid>
                  {listings.items.map((product) => (
                    <ProductCard key={product.id} product={product} />
                  ))}
                </ProductGrid>

                {/* Pagination */}
                {listings.totalPages > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from({ length: listings.totalPages }, (_, i) => i + 1).map((p) => (
                      <a
                        key={p}
                        href={`?page=${p}`}
                        className={`px-4 py-2 rounded border ${
                          p === page
                            ? 'bg-black text-white'
                            : 'bg-white text-black hover:bg-gray-100'
                        }`}
                      >
                        {p}
                      </a>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>

          {/* Back Links */}
          <div className="mt-12 flex gap-4">
            <a href="/akcija" className="text-blue-600 hover:underline">
              ← Sve akcije
            </a>
            <a href="/" className="text-blue-600 hover:underline">
              ← Početna
            </a>
          </div>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Kategorija nije pronađena</h1>
          <a href="/akcija" className="text-blue-600 hover:underline">
            Nazad na sve akcije
          </a>
        </div>
      </div>
    );
  }
}
