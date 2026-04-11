import type { MetadataRoute } from 'next';
import { buildStorefrontSitemap } from '@/lib/seo';

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  return buildStorefrontSitemap();
}
