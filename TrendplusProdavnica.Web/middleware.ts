import type { NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

const HOME_CACHE_CONTROL = 'public, max-age=0, s-maxage=300, stale-while-revalidate=60';
const LISTING_CACHE_CONTROL = 'public, max-age=0, s-maxage=120, stale-while-revalidate=30';
const ENTITY_CACHE_CONTROL = 'public, max-age=0, s-maxage=300, stale-while-revalidate=60';
const PRIVATE_NO_STORE = 'private, no-store, max-age=0';

const reservedTopLevelSlugs = new Set([
  'account',
  'admin',
  'akcija',
  'api',
  'brendovi',
  'checkout',
  'editorial',
  'favicon.ico',
  'kolekcije',
  'korpa',
  'omiljeno',
  'porudzbina',
  'prodavnice',
  'proizvod',
  'robots.txt',
  'search',
  'sitemap.xml',
  '_next',
]);

function isFileRequest(pathname: string): boolean {
  return /\.[a-z0-9]+$/i.test(pathname);
}

function isTopLevelCategoryPath(pathname: string): boolean {
  if (pathname === '/' || isFileRequest(pathname)) {
    return false;
  }

  const segments = pathname.split('/').filter(Boolean);
  return segments.length === 1 && !reservedTopLevelSlugs.has(segments[0]);
}

function resolveCacheControl(pathname: string): string | null {
  if (
    pathname === '/korpa' ||
    pathname.startsWith('/checkout') ||
    pathname.startsWith('/account') ||
    pathname.startsWith('/omiljeno') ||
    pathname.startsWith('/search') ||
    pathname.startsWith('/admin')
  ) {
    return PRIVATE_NO_STORE;
  }

  if (pathname === '/') {
    return HOME_CACHE_CONTROL;
  }

  if (
    pathname === '/akcija' ||
    pathname.startsWith('/akcija/') ||
    pathname.startsWith('/brendovi/') ||
    pathname.startsWith('/kolekcije/') ||
    isTopLevelCategoryPath(pathname)
  ) {
    return LISTING_CACHE_CONTROL;
  }

  if (
    pathname.startsWith('/proizvod/') ||
    pathname === '/editorial' ||
    pathname.startsWith('/editorial/') ||
    pathname === '/prodavnice' ||
    pathname.startsWith('/prodavnice/')
  ) {
    return ENTITY_CACHE_CONTROL;
  }

  return null;
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const cacheControl = resolveCacheControl(pathname);
  const response = NextResponse.next();

  if (cacheControl) {
    response.headers.set('Cache-Control', cacheControl);
  }

  return response;
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|.*\\..*).*)'],
};
