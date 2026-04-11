import type { Metadata } from 'next';
import Link from 'next/link';
import { Container } from '@/components';
import { getHomePage } from '@/lib/api';
import { buildMetadata } from '@/lib/seo';

export const revalidate = 300;

export async function generateMetadata(): Promise<Metadata> {
  try {
    const home = await getHomePage();
    return buildMetadata({
      title: home.title || 'Trendplus Prodavnica',
      description: home.seo?.seoDescription || 'Trendplus storefront za zensku obucu, brendove i nove kolekcije.',
      path: '/',
      seo: home.seo,
      type: 'website',
    });
  } catch {
    return buildMetadata({
      title: 'Trendplus Prodavnica',
      description: 'Trendplus storefront za zensku obucu, brendove i nove kolekcije.',
      path: '/',
      type: 'website',
    });
  }
}

export default async function Home() {
  try {
    await getHomePage();

    return (
      <div>
        <section className="bg-gradient-to-r from-blue-600 to-purple-600 py-20 text-white">
          <Container>
            <h1 className="mb-4 text-5xl font-bold">Trendplus Prodavnica</h1>
            <p className="text-xl text-white/90">Pronadji savrsenu obucu za svaki dan i svaki korak.</p>
          </Container>
        </section>

        <section className="border-b bg-gray-50 py-6">
          <Container>
            <div className="grid grid-cols-2 gap-4 md:grid-cols-5">
              <Link href="/akcija" className="text-center font-semibold hover:underline">
                Akcija
              </Link>
              <Link href="/brendovi/tamaris" className="text-center font-semibold hover:underline">
                Brendovi
              </Link>
              <Link href="/kolekcije/novo" className="text-center font-semibold hover:underline">
                Kolekcije
              </Link>
              <Link href="/editorial" className="text-center font-semibold hover:underline">
                Editorial
              </Link>
              <Link href="/prodavnice" className="text-center font-semibold hover:underline">
                Prodavnice
              </Link>
            </div>
          </Container>
        </section>

        <section className="py-16">
          <Container>
            <h2 className="mb-8 text-3xl font-bold">Dobrodosli u Trendplus</h2>
            <p className="max-w-2xl text-gray-600">
              Otkrij kolekciju zenske obuce od vodecih brendova, uz brzu dostavu, jednostavan povrat i
              inspiraciju za svaki stil.
            </p>
          </Container>
        </section>
      </div>
    );
  } catch {
    return (
      <div className="py-16">
        <Container>
          <div className="text-center text-red-600">
            <h1 className="mb-2 text-2xl font-bold">Greska pri ucitavanju</h1>
            <p>Pokusajte ponovo kasnije.</p>
          </div>
        </Container>
      </div>
    );
  }
}
