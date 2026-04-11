import type { Metadata } from 'next';
import type { SeoDto } from '@/lib/types';
import {
  SEO_ALTERNATE_OPEN_GRAPH_LOCALES,
  SEO_DEFAULT_DESCRIPTION,
  SEO_DEFAULT_LOCALE,
  SEO_DEFAULT_HREFLANG,
  SEO_PRIMARY_LOCALE,
  SEO_SITE_NAME,
  SEO_SITE_URL,
  SEO_SUPPORTED_LOCALES,
  type SeoLocaleDefinition,
} from './site';

interface BuildMetadataInput {
  title: string;
  description?: string | null;
  path: string;
  seo?: SeoDto | null;
  imageUrl?: string | null;
  type?: 'website' | 'article';
  noIndex?: boolean;
  publishedTime?: string | null;
  localeCode?: string;
  localizedPaths?: Record<string, string>;
}

function normalizePath(path: string): string {
  if (!path) {
    return '/';
  }

  return path.startsWith('/') ? path : `/${path}`;
}

function isAbsoluteUrl(value: string): boolean {
  try {
    new URL(value);
    return true;
  } catch {
    return false;
  }
}

function normalizePrefix(prefix?: string): string {
  if (!prefix || prefix === '/') {
    return '';
  }

  const trimmed = prefix.trim();
  if (!trimmed) {
    return '';
  }

  return trimmed.startsWith('/') ? trimmed.replace(/\/$/, '') : `/${trimmed.replace(/\/$/, '')}`;
}

function resolveLocale(localeCode?: string): SeoLocaleDefinition {
  if (!localeCode) {
    return SEO_PRIMARY_LOCALE;
  }

  return (
    SEO_SUPPORTED_LOCALES.find(
      (locale) =>
        locale.code === localeCode ||
        locale.languageTag === localeCode ||
        locale.hrefLang === localeCode,
    ) || SEO_PRIMARY_LOCALE
  );
}

function withLocalePrefix(path: string, locale: SeoLocaleDefinition): string {
  const normalizedPath = normalizePath(path);
  const prefix = normalizePrefix(locale.pathPrefix);

  if (!prefix) {
    return normalizedPath;
  }

  if (normalizedPath === '/') {
    return prefix;
  }

  if (normalizedPath === prefix || normalizedPath.startsWith(`${prefix}/`)) {
    return normalizedPath;
  }

  return `${prefix}${normalizedPath}`;
}

function resolveLocaleUrl(locale: SeoLocaleDefinition, path: string): string {
  if (isAbsoluteUrl(path)) {
    return path;
  }

  const localizedPath = withLocalePrefix(path, locale);
  const baseUrl = (locale.baseUrl || SEO_SITE_URL).replace(/\/$/, '');
  return new URL(localizedPath, `${baseUrl}/`).toString();
}

function resolveLocalizedPath(
  locale: SeoLocaleDefinition,
  canonicalPath: string,
  localizedPaths?: Record<string, string>,
): string {
  if (!localizedPaths) {
    return canonicalPath;
  }

  return (
    localizedPaths[locale.code] ||
    localizedPaths[locale.languageTag] ||
    localizedPaths[locale.hrefLang] ||
    canonicalPath
  );
}

export function absoluteUrl(pathOrUrl: string): string {
  if (!pathOrUrl) {
    return SEO_SITE_URL;
  }

  try {
    return new URL(pathOrUrl).toString();
  } catch {
    return new URL(normalizePath(pathOrUrl), `${SEO_SITE_URL}/`).toString();
  }
}

export function resolveCanonicalUrl(path: string, seo?: SeoDto | null): string {
  return absoluteUrl(seo?.canonicalUrl || normalizePath(path));
}

export function buildLanguageAlternates(
  path: string,
  localizedPaths?: Record<string, string>,
): Record<string, string> {
  const languages: Record<string, string> = {};

  for (const locale of SEO_SUPPORTED_LOCALES) {
    const localizedPath = resolveLocalizedPath(locale, path, localizedPaths);
    languages[locale.hrefLang] = resolveLocaleUrl(locale, localizedPath);
  }

  const defaultPath = resolveLocalizedPath(SEO_PRIMARY_LOCALE, path, localizedPaths);
  languages['x-default'] = resolveLocaleUrl(SEO_PRIMARY_LOCALE, defaultPath);

  return languages;
}

function parseRobots(seo?: SeoDto | null, noIndex?: boolean): Metadata['robots'] | undefined {
  if (noIndex) {
    return {
      index: false,
      follow: false,
      googleBot: {
        index: false,
        follow: false,
      },
    };
  }

  const directive = seo?.robotsDirective?.toLowerCase();
  if (!directive) {
    return undefined;
  }

  const tokens = directive.split(',').map((token) => token.trim());
  const index = !tokens.includes('noindex');
  const follow = !tokens.includes('nofollow');

  return {
    index,
    follow,
    googleBot: {
      index,
      follow,
    },
  };
}

export function buildMetadata({
  title,
  description,
  path,
  seo,
  imageUrl,
  type = 'website',
  noIndex = false,
  publishedTime,
  localeCode,
  localizedPaths,
}: BuildMetadataInput): Metadata {
  const currentLocale = resolveLocale(localeCode);
  const resolvedTitle = seo?.seoTitle || title;
  const resolvedDescription = seo?.seoDescription || description || SEO_DEFAULT_DESCRIPTION;
  const canonical = resolveCanonicalUrl(path, seo);
  const socialTitle = seo?.ogTitle || resolvedTitle;
  const socialDescription = seo?.ogDescription || resolvedDescription;
  const socialImage = seo?.ogImageUrl || imageUrl || undefined;
  const images = socialImage ? [{ url: absoluteUrl(socialImage) }] : undefined;
  const alternateLanguages = {
    ...buildLanguageAlternates(path, localizedPaths),
    ...(seo?.alternateLanguageUrls || {}),
  };

  return {
    title: resolvedTitle,
    description: resolvedDescription,
    alternates: {
      canonical,
      languages: alternateLanguages,
    },
    robots: parseRobots(seo, noIndex),
    openGraph: {
      type,
      locale: currentLocale.openGraphLocale || SEO_DEFAULT_LOCALE,
      alternateLocale:
        SEO_ALTERNATE_OPEN_GRAPH_LOCALES.length > 0 ? SEO_ALTERNATE_OPEN_GRAPH_LOCALES : undefined,
      url: canonical,
      siteName: SEO_SITE_NAME,
      title: socialTitle,
      description: socialDescription,
      images,
      ...(type === 'article' && publishedTime
        ? {
            publishedTime,
          }
        : {}),
    },
    twitter: {
      card: images ? 'summary_large_image' : 'summary',
      title: socialTitle,
      description: socialDescription,
      images: socialImage ? [absoluteUrl(socialImage)] : undefined,
    },
    other: {
      'og:locale': currentLocale.openGraphLocale || SEO_DEFAULT_LOCALE,
      'content-language': currentLocale.languageTag || SEO_DEFAULT_HREFLANG,
    },
  };
}

export function buildNoIndexMetadata(title: string, description: string, path: string): Metadata {
  return buildMetadata({
    title,
    description,
    path,
    noIndex: true,
  });
}
