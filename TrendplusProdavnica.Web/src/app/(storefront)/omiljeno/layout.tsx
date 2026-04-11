import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata(
  'Omiljeno',
  'Wishlist stranica nije namenjena indeksiranju.',
  '/omiljeno',
);

export default function WishlistLayout({ children }: { children: ReactNode }) {
  return children;
}
