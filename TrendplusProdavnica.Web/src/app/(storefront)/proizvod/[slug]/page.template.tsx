// @ts-nocheck
/**
 * Product Detail Page Template
 * Used for: /proizvod/[slug] pages
 */

import { Metadata } from 'next';
import Image from 'next/image';
import { Container, Breadcrumbs, AddToCartButton, Section } from '@/components';
import type { ProductDetailDto } from '@/lib/types';

interface ProductPageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata(
  props: ProductPageProps
): Promise<Metadata> {
  const params = await props.params;

  // TODO: Fetch product data
  // const response = await fetch(
  //   `${process.env.NEXT_PUBLIC_API_URL}/api/catalog/product/${params.slug}`
  // );
  // const product = await response.json();

  const product = {
    name: 'Product Name',
    seo: { seoTitle: 'Product | Trendplus', seoDescription: 'Description' },
  };

  return {
    title: product.seo?.seoTitle || 'Proizvod | Trendplus',
    description: product.seo?.seoDescription || '',
  };
}

export default async function ProductPage(props: ProductPageProps) {
  const params = await props.params;

  // TODO: Fetch product data from API
  // const response = await fetch(
  //   `${process.env.NEXT_PUBLIC_API_URL}/api/catalog/product/${params.slug}`,
  //   { next: { revalidate: 300 } }
  // );
  // const product: ProductDetailDto = await response.json();

  const product = {
    id: '1',
    name: 'Premium Shoe',
    slug: params.slug,
    brand: 'Nike',
    price: 12000,
    oldPrice: 15000,
    description: 'High quality shoe',
    images: [],
    variants: [],
    breadcrumb: [{ name: 'Home', slug: '/' }],
  };

  return (
    <div>
      {/* Breadcrumb */}
      <Container>
        <div className="py-6 md:py-8">
          <Breadcrumbs
            items={(product.breadcrumb || []).map((item) => ({
              label: item.name,
              href: item.slug,
            }))}
          />
        </div>
      </Container>

      {/* Product Content */}
      <Section>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-12">
          {/* Images */}
          <div>
            <div className="bg-gray-100 aspect-square rounded-lg overflow-hidden mb-4">
              {product.images?.[0] && (
                <Image
                  src={product.images[0]}
                  alt={product.name}
                  width={600}
                  height={600}
                  className="w-full h-full object-cover"
                  priority
                />
              )}
            </div>

            {/* Thumbnails */}
            <div className="grid grid-cols-4 gap-3">
              {/* TODO: Render other images */}
            </div>
          </div>

          {/* Details */}
          <div>
            {/* Brand */}
            <p className="text-xs tracking-widest text-gray-500 uppercase mb-2">
              {product.brand}
            </p>

            {/* Title */}
            <h1 className="text-4xl font-light leading-tight text-gray-900 mb-4">
              {product.name}
            </h1>

            {/* Price */}
            <div className="flex items-baseline gap-3 mb-6">
              <span className="text-2xl font-medium text-gray-900">
                {product.price.toLocaleString('sr-RS')} дин
              </span>
              {product.oldPrice && (
                <span className="text-lg text-gray-400 line-through">
                  {product.oldPrice.toLocaleString('sr-RS')} дин
                </span>
              )}
            </div>

            {/* Description */}
            <p className="text-gray-600 mb-8 leading-relaxed">
              {product.description}
            </p>

            {/* Variants - Size & Color */}
            <div className="space-y-6 mb-8">
              {/* TODO: Render size/color variants */}
            </div>

            {/* Add to Cart */}
            <div className="space-y-4">
              <AddToCartButton productId={product.id} />

              <button className="w-full border border-gray-300 text-gray-900 py-3 hover:border-gray-900 transition-colors text-sm">
                Dodaj u omiljeno
              </button>
            </div>

            {/* Additional Info */}
            <div className="mt-12 border-t border-gray-200 pt-8 space-y-6">
              <div>
                <h3 className="text-sm font-medium text-gray-900 mb-2">Dostava i vraćanja</h3>
                <p className="text-sm text-gray-600">
                  Slobodna dostava na sve porudžbine preko 10,000 дин. Vraćanja unutar 14 dana.
                </p>
              </div>
              <div>
                <h3 className="text-sm font-medium text-gray-900 mb-2">Materijali</h3>
                <p className="text-sm text-gray-600">
                  100% pravljena koža i tekstil
                </p>
              </div>
            </div>
          </div>
        </div>
      </Section>

      {/* Related Products */}
      <Section spacingTop="lg" spacingBottom="lg">
        <div className="mb-12">
          <h2 className="text-3xl font-light leading-tight text-gray-900 mb-8">
            Slični proizvodi
          </h2>
          {/* TODO: Render related products grid */}
        </div>
      </Section>
    </div>
  );
}
