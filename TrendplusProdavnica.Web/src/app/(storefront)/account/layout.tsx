import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata(
  'Account',
  'Korisnicki nalog nije namenjen indeksiranju.',
  '/account',
);

export default function AccountLayout({ children }: { children: ReactNode }) {
  return children;
}
