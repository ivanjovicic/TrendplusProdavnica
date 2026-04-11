import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata(
  'Porudzbina',
  'Stranica porudzbine nije namenjena indeksiranju.',
  '/porudzbina',
);

export default function OrderLayout({ children }: { children: ReactNode }) {
  return children;
}
