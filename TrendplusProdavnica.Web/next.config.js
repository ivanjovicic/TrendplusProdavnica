/** @type {import('next').NextConfig} */
const isProduction = process.env.NODE_ENV === 'production';
const assetPrefix = process.env.CDN_ASSET_PREFIX?.trim();

const nextConfig = {
  reactStrictMode: true,
  compress: true,
  poweredByHeader: false,
  crossOrigin: 'anonymous',
  assetPrefix: isProduction && assetPrefix ? assetPrefix.replace(/\/$/, '') : undefined,
  images: {
    loader: 'custom',
    loaderFile: './src/lib/cdn/cloudflare-image-loader.ts',
    remotePatterns: [
      {
        protocol: 'https',
        hostname: '**',
      },
      {
        protocol: 'http',
        hostname: 'localhost',
      },
    ],
  },
  async headers() {
    return [
      {
        source: '/_next/static/:path*',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
      {
        source: '/fonts/:path*',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
      {
        source: '/images/:path*',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
    ];
  },
  async redirects() {
    return [
      // 301 redirect legacy category routes to new /kategorije/ structure
      {
        source: '/:categorySlug((?!kategorije|brendovi|kolekcije|proizvod|prodavnice|akcija|editorial|admin|api|_next|fonts|images|CDN).*)',
        destination: '/kategorije/:categorySlug',
        permanent: true, // 301 redirect
      },
    ];
  },
};

module.exports = nextConfig;
