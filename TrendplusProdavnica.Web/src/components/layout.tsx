'use client';

import React from 'react';
import Link from 'next/link';

export function Header() {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <Link href="/" className="text-2xl font-bold">
            Trendplus
          </Link>
          <nav className="flex gap-8">
            <Link href="/" className="hover:underline">Početna</Link>
            <Link href="/brendovi/tamaris" className="hover:underline">Brendovi</Link>
            <Link href="/akcija" className="hover:underline">Akcija</Link>
            <Link href="/prodavnice" className="hover:underline">Prodavnice</Link>
          </nav>
          <Link href="/korpa" className="hover:underline">🛒 Korpa</Link>
        </div>
      </div>
    </header>
  );
}

export function Footer() {
  return (
    <footer className="bg-gray-900 text-white py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <h3 className="font-bold text-lg mb-4">O nama</h3>
            <p className="text-gray-400">Top brend obuće od 1987</p>
          </div>
          <div>
            <h3 className="font-bold text-lg mb-4">Linkovi</h3>
            <ul className="space-y-2 text-gray-400">
              <li><Link href="#" className="hover:text-white">Kontakt</Link></li>
              <li><Link href="#" className="hover:text-white">Dostava</Link></li>
              <li><Link href="#" className="hover:text-white">Vraćanja</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold text-lg mb-4">Prodavnice</h3>
            <ul className="space-y-2 text-gray-400">
              <li><Link href="/prodavnice/beograd" className="hover:text-white">Beograd</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold text-lg mb-4">Vezanost</h3>
            <p className="text-gray-400">© 2026 Trendplus. Sva prava zadržana.</p>
          </div>
        </div>
      </div>
    </footer>
  );
}

export function Container({ children }: { children: React.ReactNode }) {
  return <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">{children}</div>;
}
