import { getBrandPage, getBrandListing } from '@/lib/api';
import { Breadcrumbs, EmptyState, ProductGrid, ProductCard } from '@/components';
import { Metadata } from 'next';

interface BrandPageProps {
  params: Promise<{ slug: string }>;
  searchParams?: Promise<{ page?: string }>;
}

export async function generateMetadata({ params }: BrandPageProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const brand = await getBrandPage(slug);
    return {
      title: brand.seo?.seoTitle || `${brand.name} - Brand`,
      description: brand.seo?.seoDescription || brand.shortDescription || brand.longDescription,
      openGraph: {
        title: brand.name,
        description: brand.shortDescription || brand.longDescription,
        images: brand.logoUrl ? [{ url: brand.logoUrl }] : brand.coverImageUrl ? [{ url: brand.coverImageUrl }] : [],
      },
    };
  } catch (error) {
    return {
      title: 'Brand Not Found',
    };
  }
}

export default async function BrandPage({ params, searchParams }: BrandPageProps) {
  try {
    const { slug } = await params;
    const resolvedSearchParams = searchParams ? await searchParams : {};
    const brand = await getBrandPage(slug);
    const page = parseInt(resolvedSearchParams?.page || '1');
    
    const listings = await getBrandListing(slug, {
      page,
      pageSize: 20,
    });

    const breadcrumbs = [
      { label: 'Početna', slug: '', url: '/' },
      { label: 'Brendovi', slug: 'brendovi', url: '/' },
      { label: brand.name, slug: slug, url: `/brendovi/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Hero Section */}
          <div className="mb-12">
            {brand.logoUrl && (
              <img
                src={brand.logoUrl}
                alt={brand.name}
                className="h-16 mb-6"
              />
            )}
            <h1 className="text-4xl font-bold mb-4">{brand.name}</h1>
            <p className="text-gray-600 text-lg max-w-2xl">{brand.longDescription || brand.shortDescription || brand.content}</p>
          </div>

          {/* Breadcrumbs */}
          <Breadcrumbs items={breadcrumbs} />

          {/* Products */}
          <div className="mt-12">
            <h2 className="text-2xl font-bold mb-8">Proizvodi</h2>
            {listings.products.length === 0 ? (
              <EmptyState />
            ) : (
              <>
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
          <h1 className="text-2xl font-bold mb-4">Brand nije pronađen</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Nazad na početnu
          </a>
        </div>
      </div>
    );
  }
}
