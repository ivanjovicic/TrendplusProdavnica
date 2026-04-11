import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { buildNoIndexMetadata } from '@/lib/seo';

export const metadata: Metadata = buildNoIndexMetadata(
  'Checkout',
  'Checkout stranica nije namenjena indeksiranju.',
  '/checkout',
);

export default function CheckoutLayout({ children }: { children: ReactNode }) {
  return children;
}
