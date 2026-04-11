'use client';

import { useState } from 'react';
import Link from 'next/link';

const MOBILE_NAV_ITEMS = [
  { href: '/', label: 'Pocetna' },
  { href: '/search', label: 'Pretraga' },
  { href: '/brendovi/tamaris', label: 'Brendovi' },
  { href: '/akcija', label: 'Akcija' },
  { href: '/prodavnice', label: 'Prodavnice' },
];

export function StorefrontMobileMenu() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  return (
    <div className="sm:hidden">
      <button
        onClick={() => setMobileMenuOpen((current) => !current)}
        className="text-gray-900"
        aria-label="Meni"
        aria-expanded={mobileMenuOpen}
      >
        <svg
          className="h-6 w-6"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d={
              mobileMenuOpen
                ? 'M6 18L18 6M6 6l12 12'
                : 'M4 6h16M4 12h16M4 18h16'
            }
          />
        </svg>
      </button>

      {mobileMenuOpen && (
        <nav className="absolute left-0 right-0 top-full border-t border-gray-200 bg-white px-4 py-4 shadow-sm">
          <div className="space-y-4">
            {MOBILE_NAV_ITEMS.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="block text-sm text-gray-600 hover:text-gray-900"
                onClick={() => setMobileMenuOpen(false)}
              >
                {item.label}
              </Link>
            ))}
          </div>
        </nav>
      )}
    </div>
  );
}
