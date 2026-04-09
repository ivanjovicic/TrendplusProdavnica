import Link from 'next/link';
import type { ProductCardDto } from '@/lib/types';
import { formatPrice } from '@/lib/utils/helpers';

interface ProductCardProps {
  product: ProductCardDto;
}

export function ProductCard({ product }: ProductCardProps) {
  return (
    <div className="bg-white rounded-lg overflow-hidden hover:shadow-lg transition-shadow">
      {product.primaryImageUrl && (
        <div className="aspect-square bg-gray-200 overflow-hidden">
          <img
            src={product.primaryImageUrl}
            alt={product.name}
            className="w-full h-full object-cover hover:scale-105 transition-transform"
          />
        </div>
      )}
      <div className="p-4">
        <p className="text-sm text-gray-600">{product.brandName}</p>
        <Link href={`/proizvod/${product.slug}`} className="block">
          <h3 className="font-semibold text-lg hover:underline">{product.name}</h3>
        </Link>
        <p className="text-sm text-gray-600 mb-2">{product.subtitle}</p>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="font-bold text-lg">{formatPrice(product.price)}</span>
            {product.oldPrice && (
              <span className="text-gray-400 line-through text-sm">{formatPrice(product.oldPrice)}</span>
            )}
          </div>
          {product.isNew && <span className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded">NOVO</span>}
          {product.isOnSale && <span className="text-xs bg-red-100 text-red-700 px-2 py-1 rounded">AKCIJA</span>}
        </div>
      </div>
    </div>
  );
}

interface ProductGridProps {
  products: ProductCardDto[];
}

export function ProductGrid({ products }: ProductGridProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
}
