import { getSaleListing } from '@/lib/api';
import { Breadcrumbs, EmptyState, ProductGrid } from '@/components';
import { Metadata } from 'next';

interface SalePageProps {
  searchParams?: { page?: string; sort?: string };
}

export const metadata: Metadata = {
  title: 'Akcija - Sniženi Proizvodi',
  description: 'Pronađi najbolje snižene proizvode',
};

export default async function SalePage({ searchParams }: SalePageProps) {
  try {
    const page = parseInt(searchParams?.page || '1');
    const sort = searchParams?.sort || 'newest';

    const listings = await getSaleListing({
      page,
      pageSize: 20,
      sort,
    });

    const breadcrumbs = [
      { label: 'Početna', url: '/' },
      { label: 'Akcija', url: '/akcija' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold mb-2">AKCIJA</h1>
          <p className="text-red-600 text-lg font-semibold mb-8">
            Sniženi proizvodi do {Math.max(...listings.items.map(p => p.discount || 0))}% OFF
          </p>

          <Breadcrumbs items={breadcrumbs} />

          {/* Sort Options */}
          <div className="flex gap-4 my-8">
            <a
              href="?sort=newest"
              className={`px-4 py-2 rounded border ${
                sort === 'newest' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Najnovije
            </a>
            <a
              href="?sort=discount"
              className={`px-4 py-2 rounded border ${
                sort === 'discount' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Najveće Sniženje
            </a>
            <a
              href="?sort=price-asc"
              className={`px-4 py-2 rounded border ${
                sort === 'price-asc' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Cena: Nizak → Visok
            </a>
            <a
              href="?sort=price-desc"
              className={`px-4 py-2 rounded border ${
                sort === 'price-desc' ? 'bg-black text-white' : 'bg-white text-black'
              }`}
            >
              Cena: Visok → Nizak
            </a>
          </div>

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
                        href={`?page=${p}&sort=${sort}`}
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
