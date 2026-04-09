import { Container, ProductGrid, EmptyState } from '@/components';
import { getProductDetail } from '@/lib/api';
import { AddToCartButton } from '@/components/add-to-cart';
import { formatPrice } from '@/lib/utils/helpers';
import Image from 'next/image';

interface PageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: PageProps) {
  try {
    const { slug } = await params;
    const product = await getProductDetail(slug);
    return {
      title: product.seo?.seoTitle || product.name,
      description: product.seo?.seoDescription || product.shortDescription,
    };
  } catch {
    return { title: 'Proizvod' };
  }
}

export default async function ProductPage({ params }: PageProps) {
  try {
    const { slug } = await params;
    const product = await getProductDetail(slug);

    return (
      <div>
        <Container>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 py-8">
            {/* Gallery */}
            <div className="space-y-4">
              {product.media.filter(m => m.isPrimary).map((media) => (
                <div key={media.id} className="aspect-square bg-gray-100 rounded-lg overflow-hidden">
                  <img src={media.url} alt={media.altText} className="w-full h-full object-cover" />
                </div>
              ))}
            </div>

            {/* Details */}
            <div>
              <h1 className="text-4xl font-bold mb-2">{product.name}</h1>
              <p className="text-xl text-gray-600 mb-4">{product.brandName}</p>
              
              <div className="mb-6">
                <span className="text-3xl font-bold">{formatPrice(product.price)}</span>
                {product.oldPrice && (
                  <span className="ml-4 text-gray-500 line-through">{formatPrice(product.oldPrice)}</span>
                )}
              </div>

              <p className="text-gray-700 mb-6">{product.longDescription || product.shortDescription}</p>

              {product.sizes.length > 0 && (
                <div className="mb-6">
                  <label className="block text-sm font-semibold mb-2">Veličina</label>
                  <div className="grid grid-cols-4 gap-2">
                    {product.sizes.map((size) => (
                      <button
                        key={size.sku}
                        className="border rounded py-2 hover:border-black transition-colors"
                        disabled={!size.isActive}
                      >
                        {size.sizeEu}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              <AddToCartButton variantId={product.sizes[0]?.sku.split('-')[0] as any} />

              {product.deliveryInfo && (
                <div className="mt-8 p-4 bg-blue-50 rounded">
                  <p className="text-sm text-gray-700">{product.deliveryInfo}</p>
                </div>
              )}
            </div>
          </div>

          {/* Related Products  */}
          {product.relatedByBrand && product.relatedByBrand.length > 0 && (
            <div className="py-12 border-t mt-12">
              <h2 className="text-2xl font-bold mb-6">Više od {product.brandName}</h2>
              <ProductGrid products={product.relatedByBrand.slice(0, 4)} />
            </div>
          )}
        </Container>
      </div>
    );
  } catch (error) {
    return <Container><EmptyState /></Container>;
  }
}
