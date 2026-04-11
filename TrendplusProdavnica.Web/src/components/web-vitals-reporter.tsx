'use client';

import { useReportWebVitals } from 'next/web-vitals';

const shouldReport =
  process.env.NODE_ENV === 'development' ||
  process.env.NEXT_PUBLIC_ENABLE_WEB_VITALS_LOGGING === '1';

export function WebVitalsReporter() {
  useReportWebVitals((metric) => {
    if (!shouldReport || typeof window === 'undefined') {
      return;
    }

    const payload = {
      id: metric.id,
      name: metric.name,
      value: metric.value,
      rating: metric.rating,
      navigationType: metric.navigationType,
      path: `${window.location.pathname}${window.location.search}`,
      timestampUtc: new Date().toISOString(),
    };

    const body = JSON.stringify(payload);

    if (navigator.sendBeacon) {
      navigator.sendBeacon('/api/telemetry/web-vitals', new Blob([body], { type: 'application/json' }));
      return;
    }

    void fetch('/api/telemetry/web-vitals', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body,
      keepalive: true,
      cache: 'no-store',
    });
  });

  return null;
}
