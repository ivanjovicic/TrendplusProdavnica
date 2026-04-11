# E-Commerce Analytics Pipeline - Implementation Guide

## Overview

Kompletna implementacija ecommerce analytics sistema sa:
- Event collection (product_view, product_click, add_to_cart, checkout_started, order_completed)
- Real-time event tracking frontend i backend
- PostgreSQL analytics schema
- Dashboard metrrika (conversion rate, top products, revenue per category)

## Architecture

```
Frontend/Backend Events
    ↓
POST /api/analytics/track
    ↓
AnalyticsController
    ↓
IAnalyticsService
    ↓
AnalyticsEvent (Domain)
    ↓
PostgreSQL analytics schema
    ↓
Dashboard Queries
    ↓
Metrics (conversion, top products, revenue)
```

## Event Types

```csharp
public enum AnalyticsEventType
{
    ProductView = 1,      // Korisnik je pogledao proizvod
    ProductClick = 2,     // Korisnik je kliknuo na proizvod
    AddToCart = 3,        // Korisnik je dodao proizvod u korpu
    CheckoutStarted = 4,  // Korisnik je započeo checkout
    OrderCompleted = 5    // Porudžbina je gotova
}
```

## Event Model

```csharp
public class AnalyticsEvent : EntityBase
{
    public AnalyticsEventType EventType { get; set; }
    public long? ProductId { get; set; }
    public long? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTimeOffset EventTimestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? PageUrl { get; set; }
    public string? ReferrerUrl { get; set; }
    public string? EventData { get; set; } // JSON za dodatne podatke
}
```

## Database Schema

```sql
-- Analytics schema
CREATE SCHEMA analytics;

-- Analytics events table
CREATE TABLE analytics.analytics_events (
    Id BIGSERIAL PRIMARY KEY,
    EventType SMALLINT NOT NULL,
    ProductId BIGINT,
    UserId BIGINT,
    SessionId VARCHAR(256),
    EventTimestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    IpAddress VARCHAR(45),
    UserAgent VARCHAR(500),
    PageUrl VARCHAR(2048),
    ReferrerUrl VARCHAR(2048),
    EventData JSONB,
    CreatedAtUtc TIMESTAMP WITH TIME ZONE NOT NULL,
    UpdatedAtUtc TIMESTAMP WITH TIME ZONE,
    Version BIGINT NOT NULL
);

-- Performance indexes
CREATE INDEX IX_analytics_events_timestamp ON analytics.analytics_events(EventTimestamp);
CREATE INDEX IX_analytics_events_eventtype ON analytics.analytics_events(EventType);
CREATE INDEX IX_analytics_events_productid ON analytics.analytics_events(ProductId);
CREATE INDEX IX_analytics_events_userid ON analytics.analytics_events(UserId);
CREATE INDEX IX_analytics_events_sessionid ON analytics.analytics_events(SessionId);
CREATE INDEX IX_analytics_events_type_timestamp ON analytics.analytics_events(EventType, EventTimestamp);
CREATE INDEX IX_analytics_events_product_eventtype ON analytics.analytics_events(ProductId, EventType);
```

## API Endpoints

### 1. Track Single Event (POST)

**Endpoint:** `POST /api/analytics/track`

**Access:** Public (AllowAnonymous)

**Request:**
```json
{
  "eventType": 1,
  "productId": 123,
  "sessionId": "session-uuid-abc123",
  "pageUrl": "http://localhost:3000/proizvod/cipela-1",
  "referrerUrl": "http://localhost:3000/cipele",
  "eventData": "{\"price\": 150.00, \"category\": \"salonke\"}"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "eventType": 1,
  "productId": 123,
  "userId": null,
  "sessionId": "session-uuid-abc123",
  "eventTimestamp": "2026-04-10T14:30:00Z",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "pageUrl": "http://localhost:3000/proizvod/cipela-1",
  "referrerUrl": "http://localhost:3000/cipele",
  "eventData": "{\"price\": 150.00, \"category\": \"salonke\"}"
}
```

### 2. Track Batch Events (POST)

**Endpoint:** `POST /api/analytics/track-batch`

**Access:** Public

**Request:**
```json
[
  {
    "eventType": 1,
    "productId": 123,
    "sessionId": "session-uuid-abc123",
    "pageUrl": "http://localhost:3000/proizvod/cipela-1"
  },
  {
    "eventType": 3,
    "productId": 123,
    "sessionId": "session-uuid-abc123",
    "pageUrl": "http://localhost:3000/proizvod/cipela-1",
    "eventData": "{\"cartValue\": 150.00}"
  }
]
```

**Response:** List od AnalyticsEventDto

### 3. Get Conversion Rate (GET)

**Endpoint:** `GET /api/analytics/metrics/conversion-rate?from=2026-04-01&to=2026-04-10`

**Access:** Admin только

**Response:**
```json
{
  "conversionRate": 2.5,
  "totalProductViews": 1000,
  "totalOrders": 25,
  "periodStart": "2026-04-01T00:00:00Z",
  "periodEnd": "2026-04-10T23:59:59Z"
}
```

### 4. Get Top Products (GET)

**Endpoint:** `GET /api/analytics/metrics/top-products?limit=10&from=2026-04-01&to=2026-04-10`

**Access:** Admin only

**Response:**
```json
[
  {
    "productId": 123,
    "productName": "Črне сонане",
    "viewCount": 450,
    "addToCartCount": 95,
    "orderCount": 25,
    "conversionRate": 5.56
  },
  {
    "productId": 456,
    "productName": "Bijele cipele",
    "viewCount": 380,
    "addToCartCount": 70,
    "orderCount": 18,
    "conversionRate": 4.74
  }
]
```

### 5. Get Category Revenue (GET)

**Endpoint:** `GET /api/analytics/metrics/category-revenue?from=2026-04-01&to=2026-04-10`

**Access:** Admin only

**Response:**
```json
[
  {
    "categoryId": 101,
    "categoryName": "Salonke",
    "orderCount": 150,
    "totalRevenue": 22500.00,
    "averageOrderValue": 150.00
  },
  {
    "categoryId": 104,
    "categoryName": "Gleznjače",
    "orderCount": 120,
    "totalRevenue": 24000.00,
    "averageOrderValue": 200.00
  }
]
```

### 6. Get Dashboard (GET)

**Endpoint:** `GET /api/analytics/dashboard?from=2026-04-01&to=2026-04-10`

**Access:** Admin only

**Response:**
```json
{
  "conversionRate": {
    "conversionRate": 2.5,
    "totalProductViews": 1000,
    "totalOrders": 25,
    "periodStart": "2026-04-01T00:00:00Z",
    "periodEnd": "2026-04-10T23:59:59Z"
  },
  "topProducts": [...],
  "categoryRevenue": [...],
  "totalEvents": 5500,
  "generatedAtUtc": "2026-04-10T14:35:00Z"
}
```

### 7. Get Events (GET)

**Endpoint:** `GET /api/analytics/events?page=1&pageSize=50&eventType=1&from=2026-04-01&to=2026-04-10`

**Access:** Admin only

**Response:**
```json
{
  "events": [
    {
      "id": 1,
      "eventType": 1,
      "productId": 123,
      "userId": null,
      "sessionId": "session-uuid",
      "eventTimestamp": "2026-04-10T14:30:00Z",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0...",
      "pageUrl": "...",
      "referrerUrl": "...",
      "eventData": "{...}"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "total": 5500,
    "totalPages": 110
  }
}
```

## Frontend Integration

### React Hook za event tracking

```typescript
// hooks/useAnalytics.ts
import { useCallback } from 'react';

export function useAnalytics() {
  const trackEvent = useCallback(async (
    eventType: number,
    productId?: number,
    eventData?: any
  ) => {
    try {
      await fetch('/api/analytics/track', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          eventType,
          productId,
          sessionId: getSessionId(),
          pageUrl: window.location.href,
          referrerUrl: document.referrer,
          eventData: eventData ? JSON.stringify(eventData) : undefined
        })
      });
    } catch (error) {
      console.error('Analytics tracking failed:', error);
    }
  }, []);

  return { trackEvent };
}

function getSessionId(): string {
  let sessionId = localStorage.getItem('analytics_session_id');
  if (!sessionId) {
    sessionId = `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    localStorage.setItem('analytics_session_id', sessionId);
  }
  return sessionId;
}
```

### Usage na produktima

```typescript
// components/ProductCard.tsx
import { useAnalytics } from '@/hooks/useAnalytics';
import { AnalyticsEventType } from '@/types/analytics';

export function ProductCard({ product }) {
  const { trackEvent } = useAnalytics();

  const handleProductView = () => {
    trackEvent(AnalyticsEventType.ProductView, product.id, {
      productName: product.name,
      price: product.price,
      category: product.category
    });
  };

  const handleAddToCart = () => {
    trackEvent(AnalyticsEventType.AddToCart, product.id, {
      cartValue: product.price,
      quantity: 1
    });
    // ... add to cart logic
  };

  useEffect(() => {
    handleProductView();
  }, [product.id]);

  return (
    <div>
      <h3>{product.name}</h3>
      <button onClick={handleAddToCart}>Dodaj u korpu</button>
    </div>
  );
}
```

## Backend Event Tracking

### Order Completion

```csharp
// In CheckoutService.cs
public async Task<Order> CreateOrderAsync(...)
{
    var order = new Order(...);
    await _db.SaveChangesAsync();

    // Track order completion
    var analyticsEvent = new AnalyticsEvent(
        AnalyticsEventType.OrderCompleted,
        productId: null,
        userId: order.UserId,
        sessionId: sessionId
    )
    {
        EventData = JsonSerializer.Serialize(new { 
            orderId = order.Id,
            totalAmount = order.TotalAmount,
            itemCount = order.OrderItems.Count
        })
    };

    _db.AnalyticsEvents.Add(analyticsEvent);
    await _db.SaveChangesAsync();

    return order;
}
```

## Testing

### 1. Test event tracking

```bash
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 1,
    "productId": 123,
    "sessionId": "test-session-123",
    "pageUrl": "http://localhost:3000/proizvod/cipela-1"
  }'
```

### 2. Batch event tracking

```bash
curl -X POST "http://localhost:5000/api/analytics/track-batch" \
  -H "Content-Type: application/json" \
  -d '[
    {"eventType": 1, "productId": 123, "sessionId": "test"},
    {"eventType": 3, "productId": 123, "sessionId": "test"}
  ]'
```

### 3. Get conversion rate

```bash
curl -X GET "http://localhost:5000/api/analytics/metrics/conversion-rate" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

### 4. Get dashboard

```bash
curl -X GET "http://localhost:5000/api/analytics/dashboard?from=2026-04-01&to=2026-04-10" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

## Performance Optimization

### Indexes Strategy
- EventTimestamp - Sortiranje po vremenu
- EventType - Filteriranje po tipu  
- ProductId - Analiza po proizvodu
- Composite (EventType, EventTimestamp) - Kompleksne query
- Composite (ProductId, EventType) - Product conversion

### Query Optimization
- Events table može biti velika - razmotriti particioniranje po datumu
- Analitika je read-heavy - može se offline procesirati
- Batch tracking umjesto pojedinačnih

### Storage
- EventData je store kao JSONB - bolje perforanse nego string
- Redoviti arhiviranja starih events-a (>90 days) u separate tabelu

## Monitoring

### Key Metrics
- Events per minute (EPM)
- Conversion rate trend
- Top products trend
- Revenue trend

### Alerts
- Conversion rate spada ispod 1%
- Zero traffic za >1 sat
- Database size >1GB

## Future Enhancements

1. Real-time dashboard sa WebSockets
2. Machine learning za anomaly detection
3. Cohort analysis
4. Attribution modeling
5. A/B testing framework
6. User journey visualization
7. Funnel analysis

## Implementation Checklist

- [ ] Database migration applied
- [ ] Event tracking API tested
- [ ] Frontend event tracking implemented
- [ ] Order completion event added
- [ ] Conversion rate calculated correctly
- [ ] Top products ranked properly
- [ ] Category revenue aggregated
- [ ] Dashboard metrics available
- [ ] Admin UI created for dashboard
- [ ] Performance monitoring setup
- [ ] Documentation updated
