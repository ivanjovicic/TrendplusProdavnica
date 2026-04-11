import { SITE_NAME, SITE_URL } from '@/lib/config/env';

export interface SeoLocaleDefinition {
  code: string;
  languageTag: string;
  hrefLang: string;
  openGraphLocale: string;
  pathPrefix?: string;
  baseUrl?: string;
  isDefault?: boolean;
}

export const SEO_SITE_NAME = SITE_NAME;
export const SEO_SITE_URL = SITE_URL.replace(/\/$/, '');
export const SEO_DEFAULT_DESCRIPTION =
  'Trendplus Prodavnica nudi zensku obucu, aktuelne kolekcije, proverene brendove i inspiraciju za svaki korak.';

const DEFAULT_LOCALE_DEFINITIONS: SeoLocaleDefinition[] = [
  {
    code: 'sr',
    languageTag: 'sr-RS',
    hrefLang: 'sr-RS',
    openGraphLocale: 'sr_RS',
    pathPrefix: '',
    isDefault: true,
  },
];

function isSeoLocaleDefinition(value: unknown): value is SeoLocaleDefinition {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const candidate = value as Partial<SeoLocaleDefinition>;
  return (
    typeof candidate.code === 'string' &&
    typeof candidate.languageTag === 'string' &&
    typeof candidate.openGraphLocale === 'string'
  );
}

function parseSeoLocales(raw?: string): SeoLocaleDefinition[] {
  if (!raw) {
    return DEFAULT_LOCALE_DEFINITIONS;
  }

  try {
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return DEFAULT_LOCALE_DEFINITIONS;
    }

    const locales = parsed
      .filter(isSeoLocaleDefinition)
      .map((locale) => ({
        ...locale,
        hrefLang: locale.hrefLang || locale.languageTag,
        pathPrefix: locale.pathPrefix || '',
      }));

    return locales.length > 0 ? locales : DEFAULT_LOCALE_DEFINITIONS;
  } catch {
    return DEFAULT_LOCALE_DEFINITIONS;
  }
}

export const SEO_SUPPORTED_LOCALES = parseSeoLocales(process.env.NEXT_PUBLIC_SEO_LOCALES);

export const SEO_PRIMARY_LOCALE =
  SEO_SUPPORTED_LOCALES.find((locale) => locale.isDefault) || SEO_SUPPORTED_LOCALES[0];

export const SEO_DEFAULT_LOCALE = SEO_PRIMARY_LOCALE.openGraphLocale;
export const SEO_DEFAULT_LANGUAGE_TAG = SEO_PRIMARY_LOCALE.languageTag;
export const SEO_DEFAULT_HREFLANG = SEO_PRIMARY_LOCALE.hrefLang;
export const SEO_ALTERNATE_OPEN_GRAPH_LOCALES = SEO_SUPPORTED_LOCALES
  .filter((locale) => locale.code !== SEO_PRIMARY_LOCALE.code)
  .map((locale) => locale.openGraphLocale);

export const STATIC_CATEGORY_PATHS = [
  '/cipele',
  '/patike',
  '/cizme',
  '/sandale',
  '/papuce',
] as const;
