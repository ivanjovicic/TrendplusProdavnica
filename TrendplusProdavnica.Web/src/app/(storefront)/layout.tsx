import type { ReactNode } from 'react';
import { Footer, Header } from '@/components/layout';
import { JsonLd, buildOrganizationJsonLd } from '@/lib/seo';

export default function StorefrontLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <JsonLd data={buildOrganizationJsonLd()} />
      <Header />
      <main>{children}</main>
      <Footer />
    </>
  );
}
