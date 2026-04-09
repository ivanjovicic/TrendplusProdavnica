import { getEditorialList } from '@/lib/api';
import { Breadcrumbs, EmptyState } from '@/components/common';
import { Metadata } from 'next';

interface EditorialListProps {
  searchParams?: Promise<{ page?: string }>;
}

export const metadata: Metadata = {
  title: 'Editorial - Blog',
  description: 'Naši članci i vodiči',
};

export default async function EditorialPage({ searchParams }: EditorialListProps) {
  try {
    const articles = await getEditorialList();

    const breadcrumbs = [
      { label: 'Početna', slug: '', url: '/' },
      { label: 'Editorial', slug: 'editorial', url: '/editorial' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold mb-4">Editorial - Blog</h1>
          <p className="text-gray-600 text-lg mb-8">
            Čitaj naše članke, vodiče i inspirativne priče
          </p>

          <Breadcrumbs items={breadcrumbs} />

          {/* Articles Grid */}
          <div className="mt-12">
            {articles.length === 0 ? (
              <EmptyState />
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                {articles.map((article) => (
                  <a
                    key={article.id}
                    href={`/editorial/${article.slug}`}
                    className="group cursor-pointer"
                  >
                    <div className="bg-gray-100 rounded-lg overflow-hidden mb-4 h-48">
                      {article.coverImageUrl ? (
                        <img
                          src={article.coverImageUrl}
                          alt={article.title}
                          className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                        />
                      ) : (
                        <div className="w-full h-full bg-gray-200" />
                      )}
                    </div>
                    <h3 className="text-lg font-semibold mb-2 group-hover:text-gray-600 transition-colors">
                      {article.title}
                    </h3>
                    <p className="text-gray-600 text-sm line-clamp-2">
                      {article.excerpt || article.body?.substring(0, 100)}
                    </p>
                    <span className="text-xs text-gray-500 mt-2 block">
                      {new Date(article.publishedAtUtc || '').toLocaleDateString('sr-RS')}
                    </span>
                  </a>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Greška pri učitavanju</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Nazad na početnu
          </a>
        </div>
      </div>
    );
  }
}
