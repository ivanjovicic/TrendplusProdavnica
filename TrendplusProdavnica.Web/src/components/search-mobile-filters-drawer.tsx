'use client';

import { useEffect, useMemo, useState } from 'react';
import { SearchFiltersForm } from '@/components/search-filters-form';
import { type SearchUiState } from '@/lib/search/search-page-state';
import type { SearchFacetsDto } from '@/lib/types';

interface SearchMobileFiltersDrawerProps {
  state: SearchUiState;
  facets: SearchFacetsDto;
}

export function SearchMobileFiltersDrawer({
  state,
  facets,
}: SearchMobileFiltersDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);
  const selectedCount = useMemo(() => {
    let count = 0;

    count += state.brands.length;
    count += state.colors.length;
    count += state.sizes.length;
    count += state.availability.length;
    count += state.minPrice != null || state.maxPrice != null ? 1 : 0;
    count += typeof state.isOnSale === 'boolean' ? 1 : 0;
    count += typeof state.isNew === 'boolean' ? 1 : 0;

    return count;
  }, [state]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  return (
    <>
      <div className="fixed inset-x-0 bottom-4 z-40 px-4 lg:hidden">
        <div className="mx-auto flex max-w-md items-center gap-3 rounded-full border border-gray-900 bg-white/95 p-2 shadow-2xl backdrop-blur">
          <button
            type="button"
            onClick={() => setIsOpen(true)}
            className="flex-1 rounded-full bg-gray-900 px-4 py-3 text-sm font-medium text-white"
          >
            Filteri i sort{selectedCount > 0 ? ` (${selectedCount})` : ''}
          </button>
        </div>
      </div>

      {isOpen ? (
        <div className="fixed inset-0 z-50 lg:hidden">
          <button
            type="button"
            aria-label="Zatvori filtere"
            onClick={() => setIsOpen(false)}
            className="absolute inset-0 bg-black/40"
          />

          <div className="absolute inset-x-0 bottom-0 max-h-[88vh] overflow-y-auto rounded-t-[2rem] bg-white shadow-2xl">
            <div className="sticky top-0 z-10 border-b border-gray-200 bg-white px-5 py-4">
              <div className="mx-auto flex max-w-lg items-center justify-between gap-4">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-gray-500">Search</p>
                  <h2 className="mt-1 text-lg font-semibold text-gray-900">Filteri i sortiranje</h2>
                </div>
                <button
                  type="button"
                  onClick={() => setIsOpen(false)}
                  className="rounded-full border border-gray-300 px-4 py-2 text-sm text-gray-700 transition-colors hover:border-gray-900 hover:text-gray-900"
                >
                  Zatvori
                </button>
              </div>
            </div>

            <div className="mx-auto max-w-lg px-4 pb-8 pt-4">
              <SearchFiltersForm
                state={state}
                facets={facets}
                onBeforeNavigate={() => setIsOpen(false)}
                className="rounded-none border-0 bg-transparent p-0 shadow-none"
              />
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}
