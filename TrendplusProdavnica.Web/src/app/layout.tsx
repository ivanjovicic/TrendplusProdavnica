import type { Metadata } from 'next';
import './globals.css';
import { Header, Footer } from '@/components/layout';

export const metadata: Metadata = {
  title: 'Trendplus Prodavnica - Brend Obuće',
  description: 'Pronađite najnovije obuće od vodećih brendova',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="sr">
      <body className="bg-white">
        <Header />
        <main>{children}</main>
        <Footer />
      </body>
    </html>
  );
}
