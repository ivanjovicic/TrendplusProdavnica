import type { Metadata } from 'next';
import './globals.css';
import { WebVitalsReporter } from '@/components/web-vitals-reporter';
import {
  SEO_DEFAULT_DESCRIPTION,
  SEO_DEFAULT_LANGUAGE_TAG,
  SEO_SITE_NAME,
  SEO_SITE_URL,
} from '@/lib/seo';

export const metadata: Metadata = {
  metadataBase: new URL(`${SEO_SITE_URL}/`),
  title: {
    default: SEO_SITE_NAME,
    template: `%s | ${SEO_SITE_NAME}`,
  },
  description: SEO_DEFAULT_DESCRIPTION,
  twitter: {
    card: 'summary',
    title: SEO_SITE_NAME,
    description: SEO_DEFAULT_DESCRIPTION,
  },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang={SEO_DEFAULT_LANGUAGE_TAG}>
      <body className="bg-white">
        <WebVitalsReporter />
        {children}
      </body>
    </html>
  );
}
