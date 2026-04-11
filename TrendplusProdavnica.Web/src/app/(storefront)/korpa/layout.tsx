import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata('Korpa', 'Stranica korpe nije namenjena indeksiranju.', '/korpa');

export default function CartLayout({ children }: { children: ReactNode }) {
  return children;
}
