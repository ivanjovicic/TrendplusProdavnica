import { permanentRedirect } from 'next/navigation';

interface LegacyCategoryPageProps {
  params: Promise<{ slug: string }>;
}

export default async function LegacyCategoryPage({ params }: LegacyCategoryPageProps) {
  const { slug } = await params;
  permanentRedirect(`/${slug}`);
}
