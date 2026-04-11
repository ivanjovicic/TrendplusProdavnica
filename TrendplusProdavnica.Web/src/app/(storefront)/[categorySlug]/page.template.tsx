// @ts-nocheck
/**
 * Category Listing Page Template
 * Used for: /cipele, /patike, /cizme, /sandale, /papuce pages
 */

import { Metadata } from 'next';
import { Container, ProductGrid, Breadcrumbs } from '@/components';
import type { ProductCardDto } from '@/lib/types';

interface CategoryPageProps {
  params: Promise<{ categorySlug: string }>;
  searchParams: Promise<{ page?: string; sort?: string }>;
}

// Server Component - Fetch data on the server
export async function generateMetadata(
  props: CategoryPageProps
): Promise<Metadata> {
  const params = await props.params;
  const categoryName = params.categorySlug.charAt(0).toUpperCase() + params.categorySlug.slice(1);

  return {
    title: `${categoryName} | Trendplus`,
    description: `Pronađi sve ${categoryName.toLowerCase()} od vodećih brendova`,
  };
}

export default async function CategoryPage(props: CategoryPageProps) {
  const params = await props.params;
  const searchParams = await props.searchParams;

  const categoryName = params.categorySlug.charAt(0).toUpperCase() + params.categorySlug.slice(1);
  const page = parseInt(searchParams.page || '1');
  const sort = searchParams.sort || 'newest';

  // Fetch products from API
  // TODO: Replace with actual API call
  // const response = await fetch(
  //   `${process.env.NEXT_PUBLIC_API_URL}/api/catalog/products?category=${categorySlug}&page=${page}&sort=${sort}`,
  //   { next: { revalidate: 60 } }
  // );
  // const products = await response.json();

  const products: ProductCardDto[] = [];

  return (
    <div>
      {/* Breadcrumb */}
      <Container>
        <div className="py-6 md:py-8">
          <Breadcrumbs
            items={[
              { label: 'Početna', href: '/' },
              { label: categoryName },
            ]}
          />
        </div>
      </Container>

      {/* Category Header */}
      <section className="border-b border-gray-200 py-12 md:py-16">
        <Container>
          <h1 className="text-4xl md:text-5xl font-light leading-tight text-gray-900 mb-4">
            {categoryName}
          </h1>
          <p className="text-lg text-gray-600">
            {products.length} proizvoda
          </p>
        </Container>
      </section>

      {/* Filters & Sorting */}
      <section className="bg-gray-50 border-b border-gray-200 py-4 md:py-6">
        <Container>
          <div className="flex justify-between items-center">
            <div className="text-sm text-gray-600">
              Prikazano {(page - 1) * 12 + 1}-{Math.min(page * 12, products.length)} od {products.length}
            </div>
            <select
              defaultValue={sort}
              className="text-sm border border-gray-300 px-3 py-2 focus:outline-none focus:border-gray-900"
            >
              <option value="newest">Najnovije</option>
              <option value="price-low">Cena: Nizaka-Visoka</option>
              <option value="price-high">Cena: Visoka-Niska</option>
              <option value="bestsellers">Najprodanije</option>
            </select>
          </div>
        </Container>
      </section>

      {/* Products */}
      <section className="py-16 md:py-24">
        <Container>
          {products.length > 0 ? (
            <ProductGrid products={products} />
          ) : (
            <div className="text-center py-12">
              <p className="text-lg text-gray-600">Nema dostupnih proizvoda</p>
            </div>
          )}
        </Container>
      </section>

      {/* Pagination */}
      {products.length > 12 && (
        <section className="py-8 md:py-12">
          <Container>
            <div className="flex justify-center items-center gap-2">
              {/* TODO: Add pagination component */}
            </div>
          </Container>
        </section>
      )}
    </div>
  );
}
