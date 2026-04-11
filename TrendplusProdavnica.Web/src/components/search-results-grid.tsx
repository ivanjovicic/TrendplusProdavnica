import Link from 'next/link';
import Image from 'next/image';
import type { SearchProductItemDto } from '@/lib/types';
import { formatPrice } from '@/lib/utils/helpers';

interface SearchResultsGridProps {
  products: SearchProductItemDto[];
}

function formatSearchPrice(product: SearchProductItemDto): string {
  if (product.minPrice == null && product.maxPrice == null) {
    return 'Cena na upit';
  }

  if (
    product.minPrice != null &&
    product.maxPrice != null &&
    product.minPrice !== product.maxPrice
  ) {
    return `${formatPrice(product.minPrice)} - ${formatPrice(product.maxPrice)}`;
  }

  return formatPrice(product.minPrice ?? product.maxPrice ?? 0);
}

function renderSearchProductImage(product: SearchProductItemDto) {
  if (!product.primaryImageUrl) {
    return <div className="absolute inset-0 bg-gray-100" aria-hidden="true" />;
  }

  return (
    <Image
      src={product.primaryImageUrl}
      alt={product.name}
      fill
      className="object-cover transition-transform duration-300 group-hover:scale-[1.02]"
      sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
      loading="lazy"
    />
  );
}

export function SearchResultsGrid({ products }: SearchResultsGridProps) {
  return (
    <div className="grid grid-cols-2 gap-6 lg:grid-cols-3 xl:grid-cols-4">
      {products.map((product) => (
        <Link key={product.productId} href={`/proizvod/${product.slug}`} className="group block">
          <article className="cursor-pointer">
            <div className="relative mb-4 aspect-square overflow-hidden bg-gray-50">
              {renderSearchProductImage(product)}

              <div className="absolute right-3 top-3 flex flex-col gap-2">
                {product.isNew && (
                  <span className="bg-black px-3 py-1 text-xs font-medium text-white">NOVO</span>
                )}
                {product.isOnSale && (
                  <span className="bg-red-600 px-3 py-1 text-xs font-medium text-white">AKCIJA</span>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <p className="text-xs uppercase tracking-widest text-gray-500">{product.brandName}</p>
              <h3 className="text-base leading-snug text-gray-900 transition-colors group-hover:text-gray-600">
                {product.name}
              </h3>
              {product.primaryColorName && (
                <p className="text-xs text-gray-500">Boja: {product.primaryColorName}</p>
              )}
              <div className="flex items-center justify-between pt-1 text-xs text-gray-500">
                <span>{product.availableSizes.length} velicina</span>
                <span>{product.inStock ? 'Na stanju' : 'Trenutno nema'}</span>
              </div>
              <div className="pt-2 text-sm font-medium text-gray-900">
                {formatSearchPrice(product)}
              </div>
            </div>
          </article>
        </Link>
      ))}
    </div>
  );
}
