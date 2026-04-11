import { NextResponse } from 'next/server';

export const dynamic = 'force-dynamic';
export const revalidate = 0;

export async function POST(request: Request) {
  const payload = await request.json().catch(() => null);

  if (payload && typeof payload === 'object') {
    console.info('[web-vitals]', JSON.stringify(payload));
  }

  return new NextResponse(null, {
    status: 204,
    headers: {
      'Cache-Control': 'no-store, max-age=0',
    },
  });
}
