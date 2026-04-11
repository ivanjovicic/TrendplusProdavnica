import Link from 'next/link';
import Image from 'next/image';

export interface BrandItem {
  id: string;
  name: string;
  slug: string;
  logo?: string;
}

interface BrandGridProps {
  brands: BrandItem[];
  columns?: 2 | 3 | 4 | 6;
}

export function BrandGrid({ brands, columns = 4 }: BrandGridProps) {
  const gridMap = {
    2: 'md:grid-cols-2',
    3: 'md:grid-cols-3',
    4: 'md:grid-cols-4',
    6: 'md:grid-cols-6',
  };

  return (
    <div className={`grid grid-cols-2 ${gridMap[columns]} gap-6 md:gap-8`}>
      {brands.map((brand) => (
        <Link
          key={brand.id}
          href={`/brendovi/${brand.slug}`}
          className="group"
        >
          <div className="flex items-center justify-center h-32 bg-gray-50 border border-gray-100 group-hover:border-gray-300 transition-colors">
            {brand.logo ? (
              <Image
                src={brand.logo}
                alt={brand.name}
                width={120}
                height={60}
                className="object-contain"
              />
            ) : (
              <span className="text-sm font-medium text-gray-600 text-center px-2">
                {brand.name}
              </span>
            )}
          </div>
        </Link>
      ))}
    </div>
  );
}
