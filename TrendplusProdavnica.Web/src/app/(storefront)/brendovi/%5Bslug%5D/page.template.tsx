// @ts-nocheck
/**
 * Brand Page Template
 * Used for: /brendovi/[slug] pages
 */

import { Metadata } from 'next';
import { Container, ProductGrid, Section, Breadcrumbs } from '@/components';
import type { ProductCardDto } from '@/lib/types';

interface BrandPageProps {
  params: Promise<{ slug: string }>;
  searchParams: Promise<{ page?: string }>;
}

export async function generateMetadata(
  props: BrandPageProps
): Promise<Metadata> {
  const params = await props.params;
  const brandName = params.slug.charAt(0).toUpperCase() + params.slug.slice(1);

  return {
    title: `${brandName} | Trendplus`,
    description: `Pronađi sve proizvode od brenda ${brandName}`,
  };
}

export default async function BrandPage(props: BrandPageProps) {
  const params = await props.params;
  const searchParams = await props.searchParams;

  const brandName = params.slug.charAt(0).toUpperCase() + params.slug.slice(1);
  const page = parseInt(searchParams.page || '1');

  // TODO: Fetch brand data and products from API
  // const response = await fetch(
  //   `${process.env.NEXT_PUBLIC_API_URL}/api/catalog/brands/${params.slug}?page=${page}`,
  //   { next: { revalidate: 60 } }
  // );
  // const data = await response.json();

  const products: ProductCardDto[] = [];

  return (
    <div>
      {/* Breadcrumb */}
      <Container>
        <div className="py-6 md:py-8">
          <Breadcrumbs
            items={[
              { label: 'Početna', href: '/' },
              { label: 'Brendovi', href: '/brendovi' },
              { label: brandName },
            ]}
          />
        </div>
      </Container>

      {/* Brand Hero */}
      <Section spacingTop="lg" spacingBottom="lg" maxWidth="lg">
        <div className="text-center max-w-2xl mx-auto">
          <h1 className="text-5xl font-light leading-tight text-gray-900 mb-6">
            {brandName}
          </h1>
          <p className="text-lg text-gray-600 leading-relaxed">
            Istražite kolekciju brenda {brandName} sa pristupom svim dostupnim proizvodima i ekskuzivnim ponudama.
          </p>
        </div>
      </Section>

      {/* Products */}
      <Section spacingBottom="lg">
        {products.length > 0 ? (
          <ProductGrid products={products} />
        ) : (
          <div className="text-center py-12">
            <p className="text-lg text-gray-600">Nema dostupnih proizvoda</p>
          </div>
        )}
      </Section>
    </div>
  );
}
