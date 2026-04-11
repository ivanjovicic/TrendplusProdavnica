import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata(
  'Search',
  'Pretraga nije namenjena indeksiranju.',
  '/search',
);

export default function SearchLayout({ children }: { children: ReactNode }) {
  return children;
}
