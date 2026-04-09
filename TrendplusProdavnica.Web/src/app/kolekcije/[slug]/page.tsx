import { getCollectionPage, getCollectionListing } from '@/lib/api';
import { Breadcrumbs, EmptyState, ProductGrid, ProductCard } from '@/components';
import { Metadata } from 'next';

interface CollectionPageProps {
  params: Promise<{ slug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

export async function generateMetadata({ params }: CollectionPageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const collection = await getCollectionPage(slug);
    return {
      title: collection.seo?.seoTitle || `${collection.name} - Collection`,
      description: collection.seo?.seoDescription || collection.longDescription || collection.shortDescription,
    };
  } catch (error) {
    return {
      title: 'Collection Not Found',
    };
  }
}

export default async function CollectionPage({ params, searchParams }: CollectionPageProps) {
  const { slug } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : {};
  try {
    const collection = await getCollectionPage(slug);
    const page = parseInt(resolvedSearchParams?.page || '1');
    
    const listings = await getCollectionListing(slug, {
      page,
      pageSize: 20,
    });

    const breadcrumbs = [
      { label: 'Početna', slug: '', url: '/' },
      { label: 'Kolekcije', slug: 'kolekcije', url: '/' },
      { label: collection.name, slug: slug, url: `/kolekcije/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Hero Section */}
          {collection.coverImageUrl && (
            <img
              src={collection.coverImageUrl}
              alt={collection.name}
              className="w-full h-96 object-cover rounded-lg mb-8"
            />
          )}

          <h1 className="text-4xl font-bold mb-4">{collection.name}</h1>
          <p className="text-gray-600 text-lg max-w-2xl mb-8">{collection.longDescription || collection.shortDescription}</p>

          {/* Breadcrumbs */}
          <Breadcrumbs items={breadcrumbs} />

          {/* Products */}
          <div className="mt-12">
            {listings.products.length === 0 ? (
              <EmptyState />
            ) : (
              <>
                <h2 className="text-2xl font-bold mb-8">Proizvodi</h2>
                <ProductGrid products={listings.products} />

                {/* Pagination */}
                {Math.ceil(listings.totalCount / listings.pageSize) > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from({ length: Math.ceil(listings.totalCount / listings.pageSize) }, (_, i) => i + 1).map((p) => (
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
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Kolekcija nije pronađena</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Nazad na početnu
          </a>
        </div>
      </div>
    );
  }
}
