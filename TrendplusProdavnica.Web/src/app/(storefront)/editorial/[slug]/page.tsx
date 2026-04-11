import type { Metadata } from 'next';
import Link from 'next/link';
import { Breadcrumbs } from '@/components/common';
import { getEditorialArticle } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

interface EditorialDetailProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: EditorialDetailProps): Promise<Metadata> {
  try {
    const { slug } = await params;
    const article = await getEditorialArticle(slug);

    return buildMetadata({
      title: article.title,
      description: article.excerpt || article.body.substring(0, 160),
      path: `/editorial/${slug}`,
      seo: article.seo,
      imageUrl: article.coverImageUrl,
      type: 'article',
      publishedTime: article.publishedAtUtc,
    });
  } catch {
    return buildMetadata({
      title: 'Editorial',
      description: 'Editorial clanak.',
      path: '/editorial',
      type: 'article',
    });
  }
}

export default async function EditorialDetailPage({ params }: EditorialDetailProps) {
  try {
    const { slug } = await params;
    const article = await getEditorialArticle(slug);

    const breadcrumbs = [
      { label: 'Pocetna', url: '/' },
      { label: 'Editorial', url: '/editorial' },
      { label: article.title, url: `/editorial/${slug}` },
    ];

    return (
      <div className="min-h-screen bg-white">
        <div className="mx-auto max-w-4xl px-4 py-8">
          <Breadcrumbs items={breadcrumbs} />

          <article className="mt-12">
            {article.coverImageUrl && (
              <img
                src={article.coverImageUrl}
                alt={article.title}
                className="mb-8 h-96 w-full rounded-lg object-cover"
              />
            )}

            <h1 className="mb-4 text-4xl font-bold">{article.title}</h1>
            <div className="mb-8 flex gap-4 text-sm text-gray-600">
              {article.publishedAtUtc && <span>{new Date(article.publishedAtUtc).toLocaleDateString('sr-RS')}</span>}
              {article.authorName && <span>{article.authorName}</span>}
            </div>

            {article.excerpt && <p className="mb-8 text-xl leading-relaxed text-gray-700">{article.excerpt}</p>}

            <div className="prose prose-lg mb-12 max-w-none" dangerouslySetInnerHTML={{ __html: article.body || '' }} />

            <Link href="/editorial" className="inline-block text-blue-600 hover:underline">
              Nazad na clanke
            </Link>
          </article>
        </div>
      </div>
    );
  } catch {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Clanak nije pronadjen</h1>
          <Link href="/editorial" className="text-blue-600 hover:underline">
            Nazad na clanke
          </Link>
        </div>
      </div>
    );
  }
}
