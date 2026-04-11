import type { MetadataRoute } from 'next';
import { absoluteUrl, SEO_SITE_URL } from '@/lib/seo';

export default function robots(): MetadataRoute.Robots {
  return {
    rules: {
      userAgent: '*',
      allow: '/',
      disallow: [
        '/cart',
        '/checkout',
        '/search',
        '/account',
        '/wishlist',
        '/korpa',
        '/omiljeno',
      ],
    },
    sitemap: absoluteUrl('/sitemap.xml'),
    host: SEO_SITE_URL,
  };
}
