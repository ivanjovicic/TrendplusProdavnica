import Link from 'next/link';
import Image from 'next/image';

export interface CategoryItem {
  id: string;
  name: string;
  slug: string;
  image?: string;
  productCount?: number;
}

interface CategoryGridProps {
  categories: CategoryItem[];
  columns?: 2 | 3 | 4;
}

export function CategoryGrid({ categories, columns = 3 }: CategoryGridProps) {
  const gridMap = {
    2: 'md:grid-cols-2',
    3: 'md:grid-cols-3',
    4: 'md:grid-cols-4',
  };

  return (
    <div className={`grid grid-cols-2 ${gridMap[columns]} gap-4 md:gap-6`}>
      {categories.map((category) => (
        <Link
          key={category.id}
          href={`/${category.slug}`}
          className="group"
        >
          <div className="relative aspect-square bg-gray-100 overflow-hidden mb-3">
            {category.image ? (
              <Image
                src={category.image}
                alt={category.name}
                fill
                className="object-cover group-hover:scale-105 transition-transform duration-300"
                sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
              />
            ) : (
              <div className="flex items-center justify-center h-full bg-gray-200">
                <span className="text-gray-400">Nema slike</span>
              </div>
            )}

            {/* Overlay */}
            <div className="absolute inset-0 bg-black/20 group-hover:bg-black/40 transition-colors" />
          </div>

          {/* Content */}
          <div className="text-center">
            <h3 className="text-sm md:text-base font-medium text-gray-900 group-hover:text-gray-600">
              {category.name}
            </h3>
            {category.productCount && (
              <p className="text-xs text-gray-500 mt-1">
                {category.productCount} proizvoda
              </p>
            )}
          </div>
        </Link>
      ))}
    </div>
  );
}
