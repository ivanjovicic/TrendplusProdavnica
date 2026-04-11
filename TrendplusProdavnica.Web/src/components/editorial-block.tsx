import Link from 'next/link';
import Image from 'next/image';

export interface EditorialBlockItem {
  id: string;
  title: string;
  subtitle?: string;
  excerpt?: string;
  slug: string;
  image?: string;
  publishedAt?: string;
}

interface EditorialBlockProps {
  items: EditorialBlockItem[];
  layout?: 'grid' | 'featured' | 'list';
  columns?: 2 | 3;
}

export function EditorialBlock({
  items,
  layout = 'grid',
  columns = 3,
}: EditorialBlockProps) {
  if (layout === 'featured' && items.length > 0) {
    const featured = items[0];
    const rest = items.slice(1);

    return (
      <div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
          {/* Featured */}
          <Link href={`/editorial/${featured.slug}`} className="group">
            <div className="relative aspect-square bg-gray-100 overflow-hidden mb-4">
              {featured.image && (
                <Image
                  src={featured.image}
                  alt={featured.title}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform duration-300"
                />
              )}
            </div>
          </Link>

          {/* Featured Content */}
          <div className="flex flex-col justify-center">
            <Link href={`/editorial/${featured.slug}`} className="group">
              <p className="text-xs tracking-widest text-gray-500 uppercase mb-3">
                Bloga
              </p>
              <h3 className="text-2xl md:text-3xl font-light leading-tight text-gray-900 mb-4 group-hover:text-gray-600">
                {featured.title}
              </h3>
            </Link>
            {featured.excerpt && (
              <p className="text-gray-600 mb-6 leading-relaxed">
                {featured.excerpt}
              </p>
            )}
            <Link
              href={`/editorial/${featured.slug}`}
              className="inline-block text-sm tracking-wide text-gray-900 hover:text-gray-600 border-b border-gray-900 pb-1 w-fit"
            >
              Pročitaj više
            </Link>
          </div>
        </div>

        {/* Rest */}
        {rest.length > 0 && (
          <div className={`grid grid-cols-1 ${columns === 2 ? 'md:grid-cols-2' : 'md:grid-cols-3'} gap-8`}>
            {rest.map((item) => (
              <EditorialCard key={item.id} item={item} />
            ))}
          </div>
        )}
      </div>
    );
  }

  const gridMap = {
    2: 'md:grid-cols-2',
    3: 'md:grid-cols-3',
  };

  return (
    <div className={`grid grid-cols-1 ${gridMap[columns]} gap-8`}>
      {items.map((item) => (
        <EditorialCard key={item.id} item={item} />
      ))}
    </div>
  );
}

function EditorialCard({ item }: { item: EditorialBlockItem }) {
  return (
    <Link href={`/editorial/${item.slug}`} className="group">
      {item.image && (
        <div className="relative aspect-[4/3] bg-gray-100 overflow-hidden mb-4">
          <Image
            src={item.image}
            alt={item.title}
            fill
            className="object-cover group-hover:scale-105 transition-transform duration-300"
          />
        </div>
      )}

      <div>
        <p className="text-xs tracking-widest text-gray-500 uppercase mb-2">
          Bloga
        </p>
        <h3 className="text-base font-medium text-gray-900 group-hover:text-gray-600 mb-2 leading-snug">
          {item.title}
        </h3>
        {item.excerpt && (
          <p className="text-sm text-gray-600 line-clamp-2 mb-3">
            {item.excerpt}
          </p>
        )}
        {item.publishedAt && (
          <p className="text-xs text-gray-500">
            {new Date(item.publishedAt).toLocaleDateString('sr-RS')}
          </p>
        )}
      </div>
    </Link>
  );
}
