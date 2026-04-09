import type { Metadata } from 'next';
import { getHomePage } from '@/lib/api';
import { Container, ProductGrid } from '@/components';
import Link from 'next/link';

export async function generateMetadata(): Promise<Metadata> {
  try {
    const home = await getHomePage();
    return {
      title: home.seo?.seoTitle || 'Trendplus Prodavnica',
      description: home.seo?.seoDescription || 'Pronađite najnovije obuće od vodećih brendova',
    };
  } catch {
    return {
      title: 'Trendplus Prodavnica',
      description: 'Pronađite najnovije obuće od vodećih brendova',
    };
  }
}

export default async function Home() {
  try {
    const home = await getHomePage();
    
    return (
      <div>
        {/* Hero Section */}
        <section className="bg-gradient-to-r from-blue-600 to-purple-600 text-white py-20">
          <Container>
            <h1 className="text-5xl font-bold mb-4">Trendplus Prodavnica</h1>
            <p className="text-xl text-white/90">Pronađite savršenu obuću za sebe</p>
          </Container>
        </section>

        {/* Navigation */}
        <section className="bg-gray-50 py-6 border-b">
          <Container>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
              <Link href="/akcija" className="text-center font-semibold hover:underline">🔥 Akcija</Link>
              <Link href="/brendovi/tamaris" className="text-center font-semibold hover:underline">Brendovi</Link>
              <Link href="/kolekcije/novo" className="text-center font-semibold hover:underline">Kolekcije</Link>
              <Link href="/editorial" className="text-center font-semibold hover:underline">Bloga</Link>
              <Link href="/prodavnice" className="text-center font-semibold hover:underline">Prodavnice</Link>
            </div>
          </Container>
        </section>

        {/* Content Sections - Will be available from API modules */}
        <section className="py-16">
          <Container>
            <h2 className="text-3xl font-bold mb-8">Welcome to Trendplus</h2>
            <p className="text-gray-600 max-w-2xl">
              Otkrij našu kolekciju kvalitetne obuće od vodećih brendova. 
              Dostava brza, vraćanja jednostavna, stil garantovan.
            </p>
          </Container>
        </section>
      </div>
    );
  } catch (error) {
    console.error('Home page error:', error);
    return (
      <div className="py-16">
        <Container>
          <div className="text-center text-red-600">
            <h1 className="text-2xl font-bold mb-2">Greška pri učitavanju</h1>
            <p>Pokušajte ponovno kasnije</p>
          </div>
        </Container>
      </div>
    );
  }
}
