'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useCallback, useEffect, useRef, useTransition } from 'react';
import {
  buildClearAllFiltersHref,
  buildClearFacetHref,
  buildSearchHrefFromFormData,
  toSearchHref,
  type SearchUiState,
} from '@/lib/search/search-page-state';
import type { SearchFacetValueDto, SearchFacetsDto } from '@/lib/types';

interface SearchFiltersFormProps {
  state: SearchUiState;
  facets: SearchFacetsDto;
  className?: string;
  onBeforeNavigate?: () => void;
}

interface SharedFacetProps {
  onClearClick?: () => void;
}

function renderFacetGroup(
  inputName: string,
  title: string,
  items: SearchFacetValueDto[],
  clearHref?: string,
  sharedProps?: SharedFacetProps,
) {
  if (items.length === 0) {
    return null;
  }

  return (
    <div className="space-y-3 border-t border-gray-200 pt-6">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-900">{title}</h2>
        {clearHref ? (
          <Link
            href={clearHref}
            onClick={sharedProps?.onClearClick}
            className="text-xs text-gray-500 transition-colors hover:text-gray-900"
          >
            Ocisti
          </Link>
        ) : null}
      </div>
      <div className="space-y-2">
        {items.map((item) => (
          <label key={`${inputName}-${item.value}`} className="flex items-center justify-between gap-3 text-sm text-gray-700">
            <span className="flex items-center gap-3">
              <input
                type="checkbox"
                name={inputName}
                value={item.value}
                defaultChecked={item.selected}
                className="h-4 w-4 border-gray-300 text-gray-900 focus:ring-gray-900"
              />
              <span>{item.label}</span>
            </span>
            <span className="text-xs text-gray-400">{item.count}</span>
          </label>
        ))}
      </div>
    </div>
  );
}

function renderBooleanFacet(
  name: 'isOnSale' | 'isNew',
  title: string,
  options: SearchFacetValueDto[],
  selectedValue?: boolean,
  clearHref?: string,
  sharedProps?: SharedFacetProps,
) {
  if (options.length === 0) {
    return null;
  }

  const trueOption = options.find((option) => option.value === 'true');
  const falseOption = options.find((option) => option.value === 'false');

  return (
    <div className="space-y-3 border-t border-gray-200 pt-6">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-900">{title}</h2>
        {clearHref ? (
          <Link
            href={clearHref}
            onClick={sharedProps?.onClearClick}
            className="text-xs text-gray-500 transition-colors hover:text-gray-900"
          >
            Ocisti
          </Link>
        ) : null}
      </div>
      <div className="space-y-2 text-sm text-gray-700">
        <label className="flex items-center gap-3">
          <input
            type="radio"
            name={name}
            value=""
            defaultChecked={typeof selectedValue !== 'boolean'}
            className="h-4 w-4 border-gray-300 text-gray-900 focus:ring-gray-900"
          />
          <span>Sve</span>
        </label>
        {trueOption ? (
          <label className="flex items-center justify-between gap-3">
            <span className="flex items-center gap-3">
              <input
                type="radio"
                name={name}
                value="true"
                defaultChecked={selectedValue === true}
                className="h-4 w-4 border-gray-300 text-gray-900 focus:ring-gray-900"
              />
              <span>{trueOption.label}</span>
            </span>
            <span className="text-xs text-gray-400">{trueOption.count}</span>
          </label>
        ) : null}
        {falseOption ? (
          <label className="flex items-center justify-between gap-3">
            <span className="flex items-center gap-3">
              <input
                type="radio"
                name={name}
                value="false"
                defaultChecked={selectedValue === false}
                className="h-4 w-4 border-gray-300 text-gray-900 focus:ring-gray-900"
              />
              <span>{falseOption.label}</span>
            </span>
            <span className="text-xs text-gray-400">{falseOption.count}</span>
          </label>
        ) : null}
      </div>
    </div>
  );
}

export function SearchFiltersForm({
  state,
  facets,
  className,
  onBeforeNavigate,
}: SearchFiltersFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const formRef = useRef<HTMLFormElement>(null);
  const debounceHandleRef = useRef<number | null>(null);
  const currentHref = toSearchHref(state);
  const handleLinkClick = useCallback(() => {
    onBeforeNavigate?.();
  }, [onBeforeNavigate]);

  const commitForm = useCallback((delayMs: number) => {
    const form = formRef.current;
    if (!form) {
      return;
    }

    if (debounceHandleRef.current != null) {
      window.clearTimeout(debounceHandleRef.current);
    }

    const runNavigation = () => {
      const nextHref = buildSearchHrefFromFormData(new FormData(form));
      if (nextHref === currentHref) {
        return;
      }

      onBeforeNavigate?.();
      startTransition(() => {
        router.replace(nextHref, { scroll: false });
      });
    };

    if (delayMs > 0) {
      debounceHandleRef.current = window.setTimeout(runNavigation, delayMs);
      return;
    }

    runNavigation();
  }, [currentHref, onBeforeNavigate, router, startTransition]);

  useEffect(() => {
    return () => {
      if (debounceHandleRef.current != null) {
        window.clearTimeout(debounceHandleRef.current);
      }
    };
  }, []);

  return (
    <form
      ref={formRef}
      method="get"
      action="/search"
      className={`space-y-6 rounded-2xl border border-gray-200 bg-white p-5 ${className || ''}`.trim()}
      onSubmit={(event) => {
        event.preventDefault();
        commitForm(0);
      }}
      onChange={(event) => {
        const target = event.target;
        if (!(target instanceof HTMLInputElement || target instanceof HTMLSelectElement)) {
          return;
        }

        const delayMs = target.name === 'minPrice' || target.name === 'maxPrice' ? 350 : 0;
        commitForm(delayMs);
      }}
    >
      {state.q ? <input type="hidden" name="q" value={state.q} /> : null}

      <div className="space-y-3">
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-900">Sort</h2>
          <span className="text-[11px] uppercase tracking-[0.16em] text-gray-400">
            {isPending ? 'Azuriram...' : 'Auto'}
          </span>
        </div>
        <select
          name="sort"
          defaultValue={state.sort}
          className="w-full border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 focus:border-gray-900 focus:outline-none"
        >
          <option value="relevance">Relevantnost</option>
          <option value="popular">Popularno</option>
          <option value="newest">Najnovije</option>
          <option value="price_asc">Cena rastuce</option>
          <option value="price_desc">Cena opadajuce</option>
          <option value="bestsellers">Bestseller</option>
        </select>
      </div>

      <div className="space-y-3 border-t border-gray-200 pt-6">
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-900">Cena</h2>
          {state.minPrice != null || state.maxPrice != null ? (
            <Link
              href={buildClearFacetHref(state, 'price')}
              onClick={handleLinkClick}
              className="text-xs text-gray-500 transition-colors hover:text-gray-900"
            >
              Ocisti
            </Link>
          ) : null}
        </div>
        <div className="grid grid-cols-2 gap-3">
          <input
            type="number"
            name="minPrice"
            min={0}
            defaultValue={state.minPrice}
            placeholder={facets.priceRange.min != null ? String(Math.floor(facets.priceRange.min)) : 'Od'}
            className="w-full border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
          />
          <input
            type="number"
            name="maxPrice"
            min={0}
            defaultValue={state.maxPrice}
            placeholder={facets.priceRange.max != null ? String(Math.ceil(facets.priceRange.max)) : 'Do'}
            className="w-full border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
          />
        </div>
      </div>

      {renderFacetGroup(
        'brands',
        'Brendovi',
        facets.brands,
        state.brands.length > 0 ? buildClearFacetHref(state, 'brands') : undefined,
        { onClearClick: handleLinkClick },
      )}
      {renderFacetGroup(
        'colors',
        'Boje',
        facets.colors,
        state.colors.length > 0 ? buildClearFacetHref(state, 'colors') : undefined,
        { onClearClick: handleLinkClick },
      )}
      {renderFacetGroup(
        'sizes',
        'Velicine',
        facets.sizes,
        state.sizes.length > 0 ? buildClearFacetHref(state, 'sizes') : undefined,
        { onClearClick: handleLinkClick },
      )}
      {renderFacetGroup(
        'availability',
        'Dostupnost',
        facets.availability,
        state.availability.length > 0 ? buildClearFacetHref(state, 'availability') : undefined,
        { onClearClick: handleLinkClick },
      )}
      {renderBooleanFacet(
        'isOnSale',
        'Akcija',
        facets.sale,
        state.isOnSale,
        typeof state.isOnSale === 'boolean' ? buildClearFacetHref(state, 'isOnSale') : undefined,
        { onClearClick: handleLinkClick },
      )}
      {renderBooleanFacet(
        'isNew',
        'Novo',
        facets.new,
        state.isNew,
        typeof state.isNew === 'boolean' ? buildClearFacetHref(state, 'isNew') : undefined,
        { onClearClick: handleLinkClick },
      )}

      <div className="border-t border-gray-200 pt-6">
        <p className="text-sm text-gray-500">Filteri se primenjuju automatski cim promenis izbor.</p>
        <Link
          href={buildClearAllFiltersHref(state)}
          onClick={handleLinkClick}
          className="mt-3 block text-center text-sm text-gray-500 transition-colors hover:text-gray-900"
        >
          Ocisti filtere
        </Link>
      </div>
    </form>
  );
}
