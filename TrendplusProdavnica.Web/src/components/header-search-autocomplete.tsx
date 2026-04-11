'use client';

import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect, useId, useMemo, useRef, useState } from 'react';
import { autocompleteProducts } from '@/lib/api';
import type { ProductAutocompleteItemDto } from '@/lib/types';

const MIN_QUERY_LENGTH = 2;
const DEBOUNCE_MS = 180;

export function HeaderSearchAutocomplete() {
  const router = useRouter();
  const [query, setQuery] = useState('');
  const [items, setItems] = useState<ProductAutocompleteItemDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isFocused, setIsFocused] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement>(null);
  const listboxId = useId();

  const normalizedQuery = query.trim();
  const showDropdown = isFocused && normalizedQuery.length >= MIN_QUERY_LENGTH;
  const resultsHref = useMemo(
    () => `/search?q=${encodeURIComponent(normalizedQuery)}`,
    [normalizedQuery],
  );
  const optionCount = !isLoading && normalizedQuery.length >= MIN_QUERY_LENGTH ? items.length + 1 : 0;

  function closeDropdown() {
    setIsFocused(false);
    setActiveIndex(-1);
  }

  function navigateToHref(href: string) {
    closeDropdown();
    router.push(href);
  }

  useEffect(() => {
    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        closeDropdown();
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        closeDropdown();
      }
    }

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, []);

  useEffect(() => {
    if (!showDropdown) {
      setActiveIndex(-1);
    }
  }, [showDropdown]);

  useEffect(() => {
    setActiveIndex(-1);
  }, [normalizedQuery]);

  useEffect(() => {
    if (activeIndex > items.length) {
      setActiveIndex(-1);
    }
  }, [activeIndex, items.length]);

  useEffect(() => {
    if (normalizedQuery.length < MIN_QUERY_LENGTH) {
      setItems([]);
      setIsLoading(false);
      setActiveIndex(-1);
      return;
    }

    const controller = new AbortController();
    const timeoutHandle = window.setTimeout(async () => {
      setIsLoading(true);

      try {
        const result = await autocompleteProducts(normalizedQuery, 6, {
          signal: controller.signal,
        });
        setItems(result.items);
      } catch (error) {
        if ((error as Error).name !== 'AbortError') {
          setItems([]);
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }, DEBOUNCE_MS);

    return () => {
      controller.abort();
      window.clearTimeout(timeoutHandle);
    };
  }, [normalizedQuery]);

  const activeDescendantId = activeIndex >= 0 ? `${listboxId}-option-${activeIndex}` : undefined;

  return (
    <div ref={containerRef} className="relative hidden lg:block">
      <form action="/search" method="get" className="flex items-center gap-2">
        <input
          type="search"
          name="q"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onFocus={() => setIsFocused(true)}
          onKeyDown={(event) => {
            if (event.key === 'ArrowDown') {
              if (optionCount < 1) {
                return;
              }

              event.preventDefault();
              setIsFocused(true);
              setActiveIndex((previousValue) => (
                previousValue >= optionCount - 1 ? 0 : previousValue + 1
              ));
              return;
            }

            if (event.key === 'ArrowUp') {
              if (optionCount < 1) {
                return;
              }

              event.preventDefault();
              setIsFocused(true);
              setActiveIndex((previousValue) => (
                previousValue <= 0 ? optionCount - 1 : previousValue - 1
              ));
              return;
            }

            if (event.key === 'Home') {
              if (optionCount < 1) {
                return;
              }

              event.preventDefault();
              setIsFocused(true);
              setActiveIndex(0);
              return;
            }

            if (event.key === 'End') {
              if (optionCount < 1) {
                return;
              }

              event.preventDefault();
              setIsFocused(true);
              setActiveIndex(optionCount - 1);
              return;
            }

            if (event.key === 'Enter' && activeIndex >= 0) {
              event.preventDefault();

              if (activeIndex < items.length) {
                navigateToHref(`/proizvod/${items[activeIndex].slug}`);
                return;
              }

              navigateToHref(resultsHref);
              return;
            }

            if (event.key === 'Tab') {
              closeDropdown();
              return;
            }

            if (event.key === 'Escape') {
              closeDropdown();
            }
          }}
          placeholder="Pretrazi"
          autoComplete="off"
          className="w-56 border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
          aria-label="Pretraga proizvoda"
          role="combobox"
          aria-expanded={showDropdown}
          aria-controls={listboxId}
          aria-activedescendant={activeDescendantId}
          aria-autocomplete="list"
        />
        <button
          type="submit"
          className="border border-gray-900 px-3 py-2 text-sm text-gray-900 transition-colors hover:bg-gray-900 hover:text-white"
        >
          Trazi
        </button>
      </form>

      {showDropdown && (
        <div
          id={listboxId}
          role="listbox"
          className="absolute right-0 top-full z-50 mt-2 w-[28rem] overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-xl"
        >
          <div className="border-b border-gray-100 px-4 py-3 text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">
            Predlozi
          </div>

          {isLoading ? (
            <div className="px-4 py-6 text-sm text-gray-500">Ucitavanje predloga...</div>
          ) : (
            <>
              {items.length > 0 ? (
                <div className="divide-y divide-gray-100">
                  {items.map((item, index) => {
                    const isActive = activeIndex === index;

                    return (
                      <Link
                        key={item.productId}
                        id={`${listboxId}-option-${index}`}
                        href={`/proizvod/${item.slug}`}
                        role="option"
                        aria-selected={isActive}
                        onMouseEnter={() => setActiveIndex(index)}
                        onClick={() => closeDropdown()}
                        className={`flex items-center gap-4 px-4 py-3 transition-colors ${
                          isActive ? 'bg-gray-50' : 'hover:bg-gray-50'
                        }`}
                      >
                        <div className="relative h-16 w-16 flex-none overflow-hidden rounded-xl bg-gray-100">
                          {item.primaryImageUrl ? (
                            <Image
                              src={item.primaryImageUrl}
                              alt={item.name}
                              fill
                              className="object-cover"
                              sizes="64px"
                            />
                          ) : null}
                        </div>
                        <div className="min-w-0">
                          <p className="text-xs uppercase tracking-[0.18em] text-gray-400">{item.brandName}</p>
                          <p className="mt-1 truncate text-sm font-medium text-gray-900">{item.name}</p>
                        </div>
                      </Link>
                    );
                  })}
                </div>
              ) : (
                <div className="px-4 py-6 text-sm text-gray-500">
                  Nema predloga. Pokusaj drugi pojam ili otvori punu pretragu.
                </div>
              )}

              <div className="border-t border-gray-100 bg-gray-50 px-4 py-3">
                <Link
                  id={`${listboxId}-option-${items.length}`}
                  href={resultsHref}
                  role="option"
                  aria-selected={activeIndex === items.length}
                  onMouseEnter={() => setActiveIndex(items.length)}
                  onClick={() => closeDropdown()}
                  className={`block text-sm font-medium transition-colors ${
                    activeIndex === items.length ? 'text-gray-600' : 'text-gray-900 hover:text-gray-600'
                  }`}
                >
                  Prikazi sve rezultate za "{normalizedQuery}"
                </Link>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}
