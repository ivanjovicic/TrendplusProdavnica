import type { Metadata } from 'next';
import { ProductDetailsClient } from '@/components/product-details-client';
import { Breadcrumbs, Container, EmptyState, ProductGrid } from '@/components';
import { getProductDetail } from '@/lib/api';
import { formatPrice } from '@/lib/utils/helpers';
import { JsonLd, buildMetadata, buildProductJsonLd } from '@/lib/seo';

interface PageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const product = await getProductDetail(slug);

    return buildMetadata({
      title: product.name,
      description: product.shortDescription,
      path: `/proizvod/${slug}`,
      seo: product.seo,
      imageUrl: product.media[0]?.url,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Proizvod',
      description: 'Detalj proizvoda.',
      path: '/proizvod',
      type: 'website',
    });
  }
}

export default async function ProductPage({ params }: PageProps) {
  try {
    const { slug } = await params;
    const product = await getProductDetail(slug);
    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: product.categoryName, url: `/kategorije/${product.categorySlug}` },
      { label: product.name, url: `/proizvod/${product.slug}` },
    ];

    return (
      <div>
        <Container>
          <JsonLd data={buildProductJsonLd(product)} />
          <div className="py-8">
            <Breadcrumbs items={breadcrumbs} />

            <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
              <div className="space-y-4">
                {product.media
                  .filter((media) => media.isPrimary)
                  .map((media) => (
                    <div key={media.id} className="aspect-square overflow-hidden rounded-lg bg-gray-100">
                      <img
                        src={media.url}
                        alt={media.altText || product.name}
                        className="h-full w-full object-cover"
                      />
                    </div>
                  ))}
              </div>

              <div>
                <p className="mb-2 text-sm uppercase tracking-wide text-gray-500">{product.brandName}</p>
                <h1 className="mb-2 text-4xl font-bold">{product.name}</h1>
                {product.subtitle && <p className="mb-4 text-lg text-gray-600">{product.subtitle}</p>}

                <div className="mb-6">
                  <span className="text-3xl font-bold">{formatPrice(product.price)}</span>
                  {product.oldPrice && (
                    <span className="ml-4 text-gray-500 line-through">{formatPrice(product.oldPrice)}</span>
                  )}
                </div>

                <p className="mb-6 text-gray-700">{product.longDescription || product.shortDescription}</p>

                <ProductDetailsClient product={product} />

                {product.deliveryInfo && (
                  <div className="mt-8 rounded bg-blue-50 p-4">
                    <p className="text-sm text-gray-700">{product.deliveryInfo}</p>
                  </div>
                )}
              </div>
            </div>
          </div>

          {product.relatedByBrand && product.relatedByBrand.length > 0 && (
            <div className="mt-12 border-t py-12">
              <h2 className="mb-6 text-2xl font-bold">Vise od brenda {product.brandName}</h2>
              <ProductGrid products={product.relatedByBrand.slice(0, 4)} />
            </div>
          )}
        </Container>
      </div>
    );
  } catch {
    return (
      <Container>
        <EmptyState />
      </Container>
    );
  }
}
