export type RawSearchParams = Record<string, string | string[] | undefined>;

export interface SelectedFilterChip {
  key: string;
  label: string;
  href: string;
}

export interface SearchUiState {
  q?: string;
  page: number;
  pageSize: number;
  sort: string;
  brands: string[];
  colors: string[];
  sizes: number[];
  minPrice?: number;
  maxPrice?: number;
  availability: string[];
  isOnSale?: boolean;
  isNew?: boolean;
}

const ALLOWED_SORTS = new Set(['relevance', 'popular', 'newest', 'price_asc', 'price_desc', 'bestsellers']);

function getFirstValue(value?: string | string[]): string | undefined {
  if (Array.isArray(value)) {
    return value.find((item) => item.trim().length > 0);
  }

  return value?.trim() ? value.trim() : undefined;
}

function getMultiValues(value?: string | string[]): string[] {
  if (!value) {
    return [];
  }

  const source = Array.isArray(value) ? value : [value];

  return source
    .flatMap((item) => item.split(','))
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

function getNumberValue(value?: string | string[]): number | undefined {
  const first = getFirstValue(value);
  if (!first) {
    return undefined;
  }

  const parsed = Number.parseFloat(first);
  return Number.isNaN(parsed) ? undefined : parsed;
}

function getBooleanValue(value?: string | string[]): boolean | undefined {
  const first = getFirstValue(value);
  if (!first) {
    return undefined;
  }

  if (first === 'true') {
    return true;
  }

  if (first === 'false') {
    return false;
  }

  return undefined;
}

function getPositiveInt(value: string | undefined, fallback: number): number {
  const parsed = Number.parseInt(value || '', 10);
  return Number.isNaN(parsed) || parsed < 1 ? fallback : parsed;
}

function buildUpdatedState(state: SearchUiState, patch: Partial<SearchUiState>): SearchUiState {
  return {
    ...state,
    ...patch,
    page: 1,
  };
}

function buildRemoveValueHref(
  state: SearchUiState,
  facet: 'brands' | 'colors' | 'availability',
  value: string,
): string {
  return toSearchHref(buildUpdatedState(state, {
    [facet]: state[facet].filter((item) => item !== value),
  } as Partial<SearchUiState>));
}

function buildRemoveSizeHref(state: SearchUiState, value: number): string {
  return toSearchHref(buildUpdatedState(state, {
    sizes: state.sizes.filter((item) => item !== value),
  }));
}

export function parseSearchState(searchParams: RawSearchParams): SearchUiState {
  const sort = getFirstValue(searchParams.sort);
  const sizes = getMultiValues(searchParams.sizes)
    .map((value) => Number.parseFloat(value))
    .filter((value) => !Number.isNaN(value) && value > 0);

  return {
    q: getFirstValue(searchParams.q),
    page: getPositiveInt(getFirstValue(searchParams.page), 1),
    pageSize: 24,
    sort: sort && ALLOWED_SORTS.has(sort) ? sort : 'relevance',
    brands: getMultiValues(searchParams.brands),
    colors: getMultiValues(searchParams.colors),
    sizes,
    minPrice: getNumberValue(searchParams.minPrice),
    maxPrice: getNumberValue(searchParams.maxPrice),
    availability: getMultiValues(searchParams.availability),
    isOnSale: getBooleanValue(searchParams.isOnSale),
    isNew: getBooleanValue(searchParams.isNew),
  };
}

export function buildSearchHrefFromFormData(formData: FormData): string {
  const rawSearchParams: RawSearchParams = {};

  for (const [key, value] of formData.entries()) {
    if (key === 'page' || typeof value !== 'string') {
      continue;
    }

    const trimmedValue = value.trim();
    if (!trimmedValue) {
      continue;
    }

    const existingValue = rawSearchParams[key];
    if (typeof existingValue === 'undefined') {
      rawSearchParams[key] = trimmedValue;
      continue;
    }

    rawSearchParams[key] = Array.isArray(existingValue)
      ? [...existingValue, trimmedValue]
      : [existingValue, trimmedValue];
  }

  return toSearchHref(parseSearchState(rawSearchParams));
}

export function toSearchHref(state: SearchUiState): string {
  const params = new URLSearchParams();

  if (state.q) {
    params.set('q', state.q);
  }

  if (state.page > 1) {
    params.set('page', String(state.page));
  }

  if (state.sort && state.sort !== 'relevance') {
    params.set('sort', state.sort);
  }

  state.brands.forEach((value) => params.append('brands', value));
  state.colors.forEach((value) => params.append('colors', value));
  state.sizes.forEach((value) => params.append('sizes', value.toString()));
  state.availability.forEach((value) => params.append('availability', value));

  if (state.minPrice != null) {
    params.set('minPrice', String(state.minPrice));
  }

  if (state.maxPrice != null) {
    params.set('maxPrice', String(state.maxPrice));
  }

  if (typeof state.isOnSale === 'boolean') {
    params.set('isOnSale', String(state.isOnSale));
  }

  if (typeof state.isNew === 'boolean') {
    params.set('isNew', String(state.isNew));
  }

  const queryString = params.toString();
  return queryString ? `/search?${queryString}` : '/search';
}

export function buildPageHref(state: SearchUiState, nextPage: number): string {
  return toSearchHref({ ...state, page: nextPage });
}

export function buildClearFacetHref(
  state: SearchUiState,
  facet: 'q' | 'brands' | 'colors' | 'sizes' | 'availability' | 'price' | 'isOnSale' | 'isNew',
): string {
  switch (facet) {
    case 'q':
      return toSearchHref(buildUpdatedState(state, { q: undefined }));
    case 'brands':
      return toSearchHref(buildUpdatedState(state, { brands: [] }));
    case 'colors':
      return toSearchHref(buildUpdatedState(state, { colors: [] }));
    case 'sizes':
      return toSearchHref(buildUpdatedState(state, { sizes: [] }));
    case 'availability':
      return toSearchHref(buildUpdatedState(state, { availability: [] }));
    case 'price':
      return toSearchHref(buildUpdatedState(state, { minPrice: undefined, maxPrice: undefined }));
    case 'isOnSale':
      return toSearchHref(buildUpdatedState(state, { isOnSale: undefined }));
    case 'isNew':
      return toSearchHref(buildUpdatedState(state, { isNew: undefined }));
    default:
      return '/search';
  }
}

export function buildClearAllFiltersHref(state: SearchUiState): string {
  return toSearchHref({
    ...state,
    page: 1,
    brands: [],
    colors: [],
    sizes: [],
    minPrice: undefined,
    maxPrice: undefined,
    availability: [],
    isOnSale: undefined,
    isNew: undefined,
  });
}

export function getAvailabilityLabel(value: string): string {
  if (value === 'in_stock') {
    return 'Na stanju';
  }

  if (value === 'out_of_stock') {
    return 'Nije na stanju';
  }

  return value;
}

function formatSize(value: number): string {
  return Number.isInteger(value) ? value.toFixed(0) : value.toString();
}

export function getSelectedFilterChips(state: SearchUiState): SelectedFilterChip[] {
  const chips: SelectedFilterChip[] = [];

  if (state.q) {
    chips.push({
      key: `q:${state.q}`,
      label: `Upit: ${state.q}`,
      href: buildClearFacetHref(state, 'q'),
    });
  }

  state.brands.forEach((value) => {
    chips.push({
      key: `brand:${value}`,
      label: `Brend: ${value}`,
      href: buildRemoveValueHref(state, 'brands', value),
    });
  });

  state.colors.forEach((value) => {
    chips.push({
      key: `color:${value}`,
      label: `Boja: ${value}`,
      href: buildRemoveValueHref(state, 'colors', value),
    });
  });

  state.sizes.forEach((value) => {
    chips.push({
      key: `size:${value}`,
      label: `Velicina: ${formatSize(value)}`,
      href: buildRemoveSizeHref(state, value),
    });
  });

  state.availability.forEach((value) => {
    chips.push({
      key: `availability:${value}`,
      label: `Dostupnost: ${getAvailabilityLabel(value)}`,
      href: buildRemoveValueHref(state, 'availability', value),
    });
  });

  if (state.minPrice != null || state.maxPrice != null) {
    const label = state.minPrice != null && state.maxPrice != null
      ? `Cena: ${state.minPrice} - ${state.maxPrice}`
      : state.minPrice != null
        ? `Cena od: ${state.minPrice}`
        : `Cena do: ${state.maxPrice}`;

    chips.push({
      key: 'price',
      label,
      href: buildClearFacetHref(state, 'price'),
    });
  }

  if (state.isOnSale === true) {
    chips.push({
      key: 'sale:true',
      label: 'Akcija',
      href: buildClearFacetHref(state, 'isOnSale'),
    });
  }

  if (state.isOnSale === false) {
    chips.push({
      key: 'sale:false',
      label: 'Redovna cena',
      href: buildClearFacetHref(state, 'isOnSale'),
    });
  }

  if (state.isNew === true) {
    chips.push({
      key: 'new:true',
      label: 'Novo',
      href: buildClearFacetHref(state, 'isNew'),
    });
  }

  if (state.isNew === false) {
    chips.push({
      key: 'new:false',
      label: 'Postojeci modeli',
      href: buildClearFacetHref(state, 'isNew'),
    });
  }

  return chips;
}

export function hasNonQueryFilters(state: SearchUiState): boolean {
  return (
    state.brands.length > 0 ||
    state.colors.length > 0 ||
    state.sizes.length > 0 ||
    state.availability.length > 0 ||
    state.minPrice != null ||
    state.maxPrice != null ||
    typeof state.isOnSale === 'boolean' ||
    typeof state.isNew === 'boolean'
  );
}
