import React from 'react';
import Link from 'next/link';
import { StorefrontMobileMenu } from './storefront-mobile-menu';
import { HeaderSearchAutocomplete } from './header-search-autocomplete';

export function Header() {
  return (
    <header className="sticky top-0 z-50 border-b border-gray-200 bg-white">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <Link href="/" className="text-lg font-light tracking-widest text-gray-900">
          TRENDPLUS
        </Link>

        <nav className="hidden items-center gap-8 sm:flex">
          <Link href="/" className="text-sm text-gray-600 transition-colors hover:text-gray-900">
            Pocetna
          </Link>
          <Link href="/brendovi/tamaris" className="text-sm text-gray-600 transition-colors hover:text-gray-900">
            Brendovi
          </Link>
          <Link href="/akcija" className="text-sm text-gray-600 transition-colors hover:text-gray-900">
            Akcija
          </Link>
          <Link href="/prodavnice" className="text-sm text-gray-600 transition-colors hover:text-gray-900">
            Prodavnice
          </Link>
        </nav>

        <div className="relative flex items-center gap-4">
          <HeaderSearchAutocomplete />
          <Link
            href="/search"
            className="text-sm text-gray-600 transition-colors hover:text-gray-900 lg:hidden"
            title="Pretraga"
          >
            Pretraga
          </Link>
          <Link
            href="/omiljeno"
            className="text-sm text-gray-600 transition-colors hover:text-gray-900"
            title="Omiljeno"
          >
            Lista zelja
          </Link>
          <Link
            href="/korpa"
            className="text-sm text-gray-600 transition-colors hover:text-gray-900"
            title="Korpa"
          >
            Korpa
          </Link>
          <StorefrontMobileMenu />
        </div>
      </div>
    </header>
  );
}

export function Footer() {
  return (
    <footer className="mt-16 border-t border-gray-200 bg-white md:mt-24">
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 md:py-20">
        <div className="mb-12 grid grid-cols-1 gap-8 sm:grid-cols-2 md:grid-cols-4">
          <div>
            <h2 className="mb-4 text-sm font-light tracking-widest text-gray-900">TRENDPLUS</h2>
            <p className="text-sm leading-relaxed text-gray-600">
              Premium obuca od vodecih svetskih brendova od 1987.
            </p>
          </div>

          <div>
            <h3 className="mb-4 text-xs font-medium uppercase tracking-widest text-gray-900">Kupi</h3>
            <ul className="space-y-3 text-sm text-gray-600">
              <li>
                <Link href="/" className="transition-colors hover:text-gray-900">
                  Pocetna
                </Link>
              </li>
              <li>
                <Link href="/brendovi/tamaris" className="transition-colors hover:text-gray-900">
                  Brendovi
                </Link>
              </li>
              <li>
                <Link href="/akcija" className="transition-colors hover:text-gray-900">
                  Akcija
                </Link>
              </li>
              <li>
                <Link href="/kolekcije/novo" className="transition-colors hover:text-gray-900">
                  Kolekcije
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h3 className="mb-4 text-xs font-medium uppercase tracking-widest text-gray-900">Pomoc</h3>
            <ul className="space-y-3 text-sm text-gray-600">
              <li>
                <Link href="#" className="transition-colors hover:text-gray-900">
                  Kontakt
                </Link>
              </li>
              <li>
                <Link href="#" className="transition-colors hover:text-gray-900">
                  Dostava
                </Link>
              </li>
              <li>
                <Link href="#" className="transition-colors hover:text-gray-900">
                  Vracanja
                </Link>
              </li>
              <li>
                <Link href="/prodavnice" className="transition-colors hover:text-gray-900">
                  Prodavnice
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h3 className="mb-4 text-xs font-medium uppercase tracking-widest text-gray-900">Novine</h3>
            <p className="mb-4 text-sm text-gray-600">Prati nove proizvode i kolekcije.</p>
            <form className="space-y-3">
              <input
                type="email"
                placeholder="Tvoja email adresa"
                className="w-full border border-gray-300 px-3 py-2 text-sm placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
              />
              <button
                type="submit"
                className="w-full bg-gray-900 py-2 text-sm text-white transition-colors hover:bg-gray-800"
              >
                Prijavi se
              </button>
            </form>
          </div>
        </div>

        <div className="border-t border-gray-200 pt-8">
          <div className="flex flex-col items-center justify-between text-sm text-gray-600 md:flex-row">
            <p>&copy; 2026 Trendplus. Sva prava zadrzana.</p>
            <div className="mt-4 flex gap-6 md:mt-0">
              <Link href="#" className="transition-colors hover:text-gray-900">
                Privatnost
              </Link>
              <Link href="#" className="transition-colors hover:text-gray-900">
                Uslovi
              </Link>
              <Link href="#" className="transition-colors hover:text-gray-900">
                Kolacici
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}

export function Container({ children }: { children: React.ReactNode }) {
  return <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">{children}</div>;
}
