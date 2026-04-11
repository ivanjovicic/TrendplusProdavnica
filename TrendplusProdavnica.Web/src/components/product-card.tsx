import Link from 'next/link';
import Image from 'next/image';
import type { ProductCardDto } from '@/lib/types';
import { formatPrice } from '@/lib/utils/helpers';

interface ProductCardProps {
  product: ProductCardDto;
}

function renderProductImage(product: ProductCardDto) {
  const primaryImageUrl = product.primaryImageUrl ?? product.secondaryImageUrl;

  if (!primaryImageUrl) {
    return <div className="absolute inset-0 bg-gray-100" aria-hidden="true" />;
  }

  if (product.primaryImageUrl && product.secondaryImageUrl) {
    return (
      <>
        <Image
          src={primaryImageUrl}
          alt={product.name}
          fill
          className="object-cover transition-opacity duration-300 group-hover:opacity-0"
          sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
          loading="lazy"
        />
        <Image
          src={product.secondaryImageUrl}
          alt={`${product.name} - dodatni prikaz`}
          fill
          className="object-cover opacity-0 transition-opacity duration-300 group-hover:opacity-100"
          sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
          loading="lazy"
        />
      </>
    );
  }

  return (
    <Image
      src={primaryImageUrl}
      alt={product.name}
      fill
      className="object-cover transition-transform duration-300 group-hover:scale-[1.02]"
      sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
      loading="lazy"
    />
  );
}

export function ProductCard({ product }: ProductCardProps) {
  return (
    <Link href={`/proizvod/${product.slug}`} className="group block">
      <article className="cursor-pointer">
        <div className="relative mb-4 aspect-square overflow-hidden bg-gray-50">
          {renderProductImage(product)}

          <div className="absolute top-3 right-3 flex flex-col gap-2">
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

          {product.subtitle && <p className="text-xs text-gray-500">{product.subtitle}</p>}

          <div className="flex items-baseline gap-2 pt-2">
            <span className="text-sm font-medium text-gray-900">{formatPrice(product.price)}</span>
            {product.oldPrice && (
              <span className="text-xs text-gray-400 line-through">{formatPrice(product.oldPrice)}</span>
            )}
          </div>
        </div>
      </article>
    </Link>
  );
}

interface ProductGridProps {
  products: ProductCardDto[];
  columns?: 'auto' | 2 | 3 | 4;
  gap?: 'sm' | 'md' | 'lg';
}

export function ProductGrid({ products, columns = 'auto', gap = 'md' }: ProductGridProps) {
  const gapClasses = {
    sm: 'gap-4',
    md: 'gap-6 lg:gap-8',
    lg: 'gap-8 lg:gap-10',
  } as const;

  const gridClasses = {
    auto: 'grid-cols-2 md:grid-cols-3 lg:grid-cols-4',
    2: 'grid-cols-2 md:grid-cols-2 lg:grid-cols-2',
    3: 'grid-cols-2 md:grid-cols-3 lg:grid-cols-3',
    4: 'grid-cols-2 md:grid-cols-3 lg:grid-cols-4',
  } as const;

  return (
    <div className={`grid ${gridClasses[columns]} ${gapClasses[gap]}`}>
      {products.map((product) => (
        <ProductCard key={product.productId ?? product.id} product={product} />
      ))}
    </div>
  );
}
