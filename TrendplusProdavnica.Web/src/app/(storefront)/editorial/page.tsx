import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs, EmptyState } from '@/components/common';
import { getEditorialList } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const metadata: Metadata = buildMetadata({
  title: 'Editorial',
  description: 'Trendplus editorial, vodici i inspiracija za zensku obucu.',
  path: '/editorial',
  type: 'website',
});

export default async function EditorialPage() {
  try {
    const articles = await getEditorialList();
    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Editorial', url: '/editorial' },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8">
          <h1 className="mb-4 text-4xl font-bold">Editorial</h1>
          <p className="mb-8 text-lg text-gray-600">Citaj nase clanke, vodice i inspirativne price.</p>

          <Breadcrumbs items={breadcrumbs} />

          <div className="mt-12">
            {articles.length === 0 ? (
              <EmptyState />
            ) : (
              <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-3">
                {articles.map((article) => (
                  <Link key={article.id} href={`/editorial/${article.slug}`} className="group cursor-pointer">
                    <div className="mb-4 h-48 overflow-hidden rounded-lg bg-gray-100">
                      {article.coverImageUrl ? (
                        <img
                          src={article.coverImageUrl}
                          alt={article.title}
                          className="h-full w-full object-cover transition-transform group-hover:scale-105"
                        />
                      ) : (
                        <div className="h-full w-full bg-gray-200" />
                      )}
                    </div>
                    <h3 className="mb-2 text-lg font-semibold transition-colors group-hover:text-gray-600">
                      {article.title}
                    </h3>
                    <p className="line-clamp-2 text-sm text-gray-600">{article.excerpt || article.body?.substring(0, 100)}</p>
                    {article.publishedAtUtc && (
                      <span className="mt-2 block text-xs text-gray-500">
                        {new Date(article.publishedAtUtc).toLocaleDateString('sr-RS')}
                      </span>
                    )}
                  </Link>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Greska pri ucitavanju</h1>
          <Link href="/" className="text-blue-600 hover:underline">
            Nazad na pocetnu
          </Link>
        </div>
      </div>
    );
  }
}
