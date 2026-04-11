type CloudflareLoaderProps = {
  src: string;
  width: number;
  quality?: number;
};

const siteUrl = (process.env.NEXT_PUBLIC_SITE_URL || '').replace(/\/$/, '');
const cdnImageBaseUrl = (process.env.NEXT_PUBLIC_CDN_IMAGE_BASE_URL || '').replace(/\/$/, '');

function normalizeSource(src: string): string {
  if (!src) {
    return src;
  }

  if (/^https?:\/\//i.test(src)) {
    return src;
  }

  if (src.startsWith('//')) {
    return `https:${src}`;
  }

  if (!siteUrl) {
    return src;
  }

  return new URL(src.startsWith('/') ? src : `/${src}`, siteUrl).toString();
}

export default function cloudflareImageLoader({
  src,
  width,
  quality,
}: CloudflareLoaderProps): string {
  const normalizedSrc = normalizeSource(src);

  if (!cdnImageBaseUrl) {
    return normalizedSrc;
  }

  const options = [
    'format=auto',
    'metadata=none',
    'fit=cover',
    `width=${Math.max(1, Math.round(width))}`,
    `quality=${quality ?? 85}`,
  ].join(',');

  return `${cdnImageBaseUrl}/${options}/${normalizedSrc}`;
}
