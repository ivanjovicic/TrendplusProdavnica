import Link from 'next/link';
import { Breadcrumbs, Container, EmptyState } from '@/components';
import { SearchFiltersForm } from '@/components/search-filters-form';
import { SearchMobileFiltersDrawer } from '@/components/search-mobile-filters-drawer';
import { SearchResultsGrid } from '@/components/search-results-grid';
import { searchProducts } from '@/lib/api';
import {
  buildClearAllFiltersHref,
  buildPageHref,
  getSelectedFilterChips,
  hasNonQueryFilters,
  parseSearchState,
  toSearchHref,
  type RawSearchParams,
  type SearchUiState,
} from '@/lib/search/search-page-state';
import type { BreadcrumbItemDto, SearchResponseDto } from '@/lib/types';

export const dynamic = 'force-dynamic';

interface SearchPageProps {
  searchParams?: Promise<RawSearchParams>;
}

function buildBreadcrumbs(): BreadcrumbItemDto[] {
  return [
    { label: 'Pocetna', url: '/' },
    { label: 'Pretraga', url: '/search' },
  ];
}

function renderPagination(data: SearchResponseDto, state: SearchUiState) {
  const totalPages = Math.ceil(data.totalCount / data.pageSize);

  if (totalPages <= 1) {
    return null;
  }

  return (
    <div className="mt-10 flex flex-wrap items-center justify-center gap-2">
      {Array.from({ length: totalPages }, (_, index) => index + 1).map((pageNumber) => (
        <Link
          key={pageNumber}
          href={buildPageHref(state, pageNumber)}
          className={`rounded border px-3 py-2 text-sm ${
            pageNumber === data.page
              ? 'border-gray-900 bg-gray-900 text-white'
              : 'border-gray-300 text-gray-700 hover:border-gray-900 hover:text-gray-900'
          }`}
        >
          {pageNumber}
        </Link>
      ))}
    </div>
  );
}

export default async function SearchPage({ searchParams }: SearchPageProps) {
  const resolvedSearchParams = (await searchParams) || {};
  const state = parseSearchState(resolvedSearchParams);
  const selectedChips = getSelectedFilterChips(state);
  const showClearAllFilters = hasNonQueryFilters(state);
  const data = await searchProducts({
    q: state.q,
    page: state.page,
    pageSize: state.pageSize,
    brands: state.brands,
    colors: state.colors,
    sizes: state.sizes,
    minPrice: state.minPrice,
    maxPrice: state.maxPrice,
    availability: state.availability,
    isOnSale: state.isOnSale,
    isNew: state.isNew,
    sort: state.sort,
  });

  return (
    <Container>
      <div className="py-8 pb-28 md:py-12 md:pb-12">
        <Breadcrumbs items={buildBreadcrumbs()} />

        <div className="mb-8 flex flex-col gap-4 border-b border-gray-200 pb-8 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-2xl">
            <p className="mb-3 text-xs font-semibold uppercase tracking-[0.25em] text-gray-500">Search</p>
            <h1 className="text-4xl font-semibold tracking-tight text-gray-900">Pretraga proizvoda</h1>
            <p className="mt-3 text-sm leading-6 text-gray-600">
              {state.q
                ? `Rezultati za "${state.q}" (${data.totalCount})`
                : `Pregled svih rezultata i faceta (${data.totalCount})`}
            </p>
          </div>

          <form method="get" action="/search" className="flex w-full max-w-2xl gap-3">
            <input
              type="search"
              name="q"
              defaultValue={state.q}
              placeholder="Pretrazi modele, brendove i kategorije"
              className="min-w-0 flex-1 border border-gray-300 px-4 py-3 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
            />
            <button
              type="submit"
              className="bg-gray-900 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-gray-800"
            >
              Pretrazi
            </button>
          </form>
        </div>

        {selectedChips.length > 0 ? (
          <div className="mb-8 rounded-2xl border border-gray-200 bg-gray-50 p-4">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
              <div className="flex flex-wrap gap-2">
                {selectedChips.map((chip) => (
                  <Link
                    key={chip.key}
                    href={chip.href}
                    className="inline-flex items-center gap-2 rounded-full border border-gray-300 bg-white px-3 py-2 text-sm text-gray-700 transition-colors hover:border-gray-900 hover:text-gray-900"
                  >
                    <span>{chip.label}</span>
                    <span aria-hidden="true" className="text-gray-400">x</span>
                  </Link>
                ))}
              </div>

              {showClearAllFilters ? (
                <Link
                  href={buildClearAllFiltersHref(state)}
                  className="text-sm font-medium text-gray-500 transition-colors hover:text-gray-900"
                >
                  Ocisti sve filtere
                </Link>
              ) : null}
            </div>
          </div>
        ) : null}

        <SearchMobileFiltersDrawer key={`mobile-${toSearchHref(state)}`} state={state} facets={data.facets} />

        <div className="grid gap-10 lg:grid-cols-[280px_minmax(0,1fr)]">
          <aside className="hidden lg:block">
            <SearchFiltersForm key={toSearchHref(state)} state={state} facets={data.facets} />
          </aside>

          <section>
            {data.products.length > 0 ? (
              <>
                <SearchResultsGrid products={data.products} />
                {renderPagination(data, state)}
              </>
            ) : (
              <EmptyState />
            )}
          </section>
        </div>
      </div>
    </Container>
  );
}
