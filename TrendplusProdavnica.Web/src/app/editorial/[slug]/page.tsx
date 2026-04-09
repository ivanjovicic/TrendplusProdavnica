import { getEditorialArticle } from '@/lib/api';
import { Breadcrumbs } from '@/components/common';
import { Metadata } from 'next';

interface EditorialDetailProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: EditorialDetailProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const article = await getEditorialArticle(slug);
    return {
      title: article.title,
      description: article.excerpt || article.content?.substring(0, 160),
    };
  } catch (error) {
    return {
      title: 'Article Not Found',
    };
  }
}

export default async function EditorialDetailPage({ params }: EditorialDetailProps) {
  try {
    const { slug } = await params;
    const article = await getEditorialArticle(slug);

    const breadcrumbs = [
      { label: 'Početna', url: '/' },
      { label: 'Editorial', url: '/editorial' },
      { label: article.title, url: `/editorial/${params.slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="max-w-4xl mx-auto px-4 py-8">
          <Breadcrumbs items={breadcrumbs} />

          {/* Article */}
          <article className="mt-12">
            {article.coverImageUrl && (
              <img
                src={article.coverImageUrl}
                alt={article.title}
                className="w-full h-96 object-cover rounded-lg mb-8"
              />
            )}

            <h1 className="text-4xl font-bold mb-4">{article.title}</h1>
            <div className="flex gap-4 text-sm text-gray-600 mb-8">
              <span>{new Date(article.publishedAtUtc || '').toLocaleDateString('sr-RS')}</span>
            </div>

            {article.excerpt && (
              <p className="text-xl text-gray-700 mb-8 leading-relaxed">
                {article.excerpt}
              </p>
            )}

            <div
              className="prose prose-lg max-w-none mb-12"
              dangerouslySetInnerHTML={{ __html: article.body || '' }}
            />

            {/* Back Link */}
            <a
              href="/editorial"
              className="inline-block text-blue-600 hover:underline"
            >
              ← Nazad na članke
            </a>
          </article>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Članak nije pronađen</h1>
          <a href="/editorial" className="text-blue-600 hover:underline">
            Nazad na članke
          </a>
        </div>
      </div>
    );
  }
}
