# Analytics Pipeline - Testing & Development Guide

## Quick Start

### 1. Apply Database Migration

```bash
cd TrendplusProdavnica.Api
dotnet ef database update
```

**Expected:** Table `analytics_events` kreirana u `analytics` schemi sa svim indexima.

### 2. Build Project

```bash
dotnet build TrendplusProdavnica.sln -c Debug
```

**Expected:** Status: Successful (no errors related to analytics)

---

## Manual API Testing

### Test 1: Track Product View Event

```bash
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 1,
    "productId": 123,
    "sessionId": "test-session-001",
    "pageUrl": "http://localhost:3000/proizvod/crne-cipele",
    "referrerUrl": "http://localhost:3000/cipele",
    "eventData": "{\"price\": 150.00, \"category\": \"salonke\"}"
  }'
```

**Expected Response (200 OK):**
```json
{
  "id": 1,
  "eventType": 1,
  "productId": 123,
  "userId": null,
  "sessionId": "test-session-001",
  "eventTimestamp": "2026-04-10T...",
  "ipAddress": "127.0.0.1",
  "userAgent": "curl/...",
  "pageUrl": "http://localhost:3000/proizvod/crne-cipele",
  "referrerUrl": "http://localhost:3000/cipele",
  "eventData": "{\"price\": 150.00, \"category\": \"salonke\"}"
}
```

---

### Test 2: Track Add to Cart Event

```bash
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 3,
    "productId": 123,
    "sessionId": "test-session-001",
    "pageUrl": "http://localhost:3000/proizvod/crne-cipele",
    "eventData": "{\"cartValue\": 150.00, \"quantity\": 1}"
  }'
```

**Expected:** Event saved sa cartValue informacijom.

---

### Test 3: Batch Event Tracking

```bash
curl -X POST "http://localhost:5000/api/analytics/track-batch" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "eventType": 1,
      "productId": 123,
      "sessionId": "test-session-001",
      "pageUrl": "http://localhost:3000/proizvod/cipela-1"
    },
    {
      "eventType": 1,
      "productId": 456,
      "sessionId": "test-session-001",
      "pageUrl": "http://localhost:3000/proizvod/cipela-2"
    },
    {
      "eventType": 3,
      "productId": 123,
      "sessionId": "test-session-001",
      "pageUrl": "http://localhost:3000/korpa"
    }
  ]'
```

**Expected:** Tri događaja kreirano odjednom.

---

### Test 4: Get Conversion Rate

**Prerequisite:** Prvo kreiraj nekoliko ProductView (eventType=1) i OrderCompleted (eventType=5) events.

```bash
# Kreiraj 10 ProductView events
for i in {1..10}; do
  curl -X POST "http://localhost:5000/api/analytics/track" \
    -H "Content-Type: application/json" \
    -d '{
      "eventType": 1,
      "productId": 123,
      "sessionId": "session-'$i'"
    }'
done

# Kreiraj 2 OrderCompleted events  
for i in {1..2}; do
  curl -X POST "http://localhost:5000/api/analytics/track" \
    -H "Content-Type: application/json" \
    -d '{
      "eventType": 5,
      "productId": 123,
      "sessionId": "session-'$i'"
    }'
done

# Get conversion rate (expects: 2 orders / 10 views = 20%)
curl -X GET "http://localhost:5000/api/analytics/metrics/conversion-rate" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response:**
```json
{
  "conversionRate": 20.0,
  "totalProductViews": 10,
  "totalOrders": 2,
  "periodStart": "2026-03-11T...",
  "periodEnd": "2026-04-10T..."
}
```

---

### Test 5: Get Top Products

```bash
# Track events for multiple products
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 1,
    "productId": 123,
    "sessionId": "session-001"
  }'

# ... (repeat za products 123, 456, 789 sa različitim brojem views)

# Get top products
curl -X GET "http://localhost:5000/api/analytics/metrics/top-products?limit=5" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response:**
```json
[
  {
    "productId": 123,
    "productName": "Crne cipele",
    "viewCount": 450,
    "addToCartCount": 95,
    "orderCount": 25,
    "conversionRate": 5.56
  },
  ...
]
```

---

### Test 6: Get Category Revenue

**Prerequisite:** Orders su već kreirani u sistemu.

```bash
curl -X GET "http://localhost:5000/api/analytics/metrics/category-revenue" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response:**
```json
[
  {
    "categoryId": 101,
    "categoryName": "Salonke",
    "orderCount": 150,
    "totalRevenue": 22500.00,
    "averageOrderValue": 150.00
  },
  ...
]
```

---

### Test 7: Get Dashboard

```bash
curl -X GET "http://localhost:5000/api/analytics/dashboard?from=2026-04-01&to=2026-04-10" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected:** Kompletan dashboard sa conversion rate, top products, category revenue i ukupan broj events.

---

### Test 8: Get Events sa Filteriranjem

```bash
# Get ProductView events (eventType=1) za zadanu datum
curl -X GET "http://localhost:5000/api/analytics/events?page=1&pageSize=20&eventType=1&from=2026-04-01&to=2026-04-10" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response:**
```json
{
  "events": [
    {
      "id": 1,
      "eventType": 1,
      "productId": 123,
      "userId": null,
      "sessionId": "session-001",
      "eventTimestamp": "2026-04-10T...",
      "ipAddress": "127.0.0.1",
      "userAgent": "curl/...",
      "pageUrl": "...",
      "referrerUrl": "...",
      "eventData": "{...}"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "total": 450,
    "totalPages": 23
  }
}
```

---

## Database Verification

### Check Created Table and Data

```sql
-- Connect to database
psql -U postgres -d trendplusprodavnica

-- List analytics schema
\dn analytics

-- Check analytics_events table
\dt analytics.*

-- View sample data
SELECT id, event_type, product_id, user_id, session_id, event_timestamp 
FROM analytics.analytics_events 
LIMIT 10;

-- Count events by type
SELECT event_type, COUNT(*) as count 
FROM analytics.analytics_events 
GROUP BY event_type;

-- Check indexes
\di analytics.*
```

---

## Frontend Integration Testing

### React Component Test

```typescript
// Test event tracking u React komponenti
import { renderHook, act } from '@testing-library/react';
import { useAnalytics } from '@/hooks/useAnalytics';

describe('useAnalytics', () => {
  it('should track product view event', async () => {
    const { result } = renderHook(() => useAnalytics());

    await act(async () => {
      await result.current.trackEvent(1, 123); // ProductView event
    });

    // Verify event was sent (check network calls)
    // expect(fetchMock).toHaveBeenCalledWith(
    //   'http://localhost:5000/api/analytics/track',
    //   expect.any(Object)
    // );
  });
});
```

---

## Load Testing

### Simulate High Event Volume

```bash
#!/bin/bash
# load-test-analytics.sh

API_URL="http://localhost:5000"
REQUESTS=1000

echo "Sending $REQUESTS analytics events..."

for i in $(seq 1 $REQUESTS); do
  curl -s -X POST "$API_URL/api/analytics/track" \
    -H "Content-Type: application/json" \
    -d "{
      \"eventType\": $((RANDOM % 5 + 1)),
      \"productId\": $((RANDOM % 100 + 1)),
      \"sessionId\": \"load-test-$i\",
      \"pageUrl\": \"http://localhost:3000/proizvod/test-$((RANDOM % 10))\",
      \"eventData\": \"{\\\"value\\\": $((RANDOM % 500))}\""
    }" &

  if [ $((i % 100)) -eq 0 ]; then
    echo "Sent $i requests..."
  fi
done

wait
echo "Load test completed!"
```

**Run:**
```bash
chmod +x load-test-analytics.sh
./load-test-analytics.sh
```

**Monitor:**
```bash
# Check database size
psql -U postgres -d trendplusprodavnica -c "
SELECT 
  pg_size_pretty(pg_total_relation_size('analytics.analytics_events')) AS table_size,
  COUNT(*) as row_count 
FROM analytics.analytics_events;"

# Check query performance
\timing on
SELECT COUNT(*) FROM analytics.analytics_events WHERE event_type = 1;
SELECT COUNT(*) FROM analytics.analytics_events WHERE product_id = 123;
```

---

## Performance Testing

### Query Performance Baseline

```bash
# Test without index usage
EXPLAIN ANALYZE SELECT * FROM analytics.analytics_events 
WHERE event_type = 1 AND event_timestamp > NOW() - INTERVAL '7 days';

# Test with composite index
EXPLAIN ANALYZE SELECT COUNT(*) FROM analytics.analytics_events 
WHERE event_type = 1 AND event_timestamp > NOW() - INTERVAL '7 days';

# Test product conversion
EXPLAIN ANALYZE 
SELECT product_id, COUNT(*) 
FROM analytics.analytics_events 
WHERE product_id = 123 
GROUP BY product_id;
```

**Expected:** Index scan (not sequential scan)

---

## Error Scenarios

### Test 1: Missing Required Fields

```bash
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 1
    # Missing other required fields
  }'
```

**Expected:** 400 Bad Request

### Test 2: Invalid Event Type

```bash
curl -X POST "http://localhost:5000/api/analytics/track" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": 99,
    "productId": 123
  }'
```

**Expected:** Event je dodan (ili validation error ako postoji)

### Test 3: Unauthorized Dashboard Access

```bash
curl -X GET "http://localhost:5000/api/analytics/dashboard"
# Without JWT token
```

**Expected:** 401 Unauthorized

### Test 4: Concurrent Event Tracking

```bash
# Send 100 concurrent requests
for i in {1..100}; do
  curl -X POST "http://localhost:5000/api/analytics/track" \
    -H "Content-Type: application/json" \
    -d '{
      "eventType": 1,
      "productId": $((i % 50 + 1))
    }' &
done
wait
```

**Expected:** Svi eventi su salvani bez duplicates ili gubitaka datos

---

## Debugging Tips

### Enable SQL Logging

```csharp
// In Program.cs
builder.Services.AddDbContext<TrendplusDbContext>(options =>
    options.UseNpgsql(connectionString)
           .LogTo(Console.WriteLine)
           .EnableSensitiveDataLogging());
```

### Check Service Registration

```bash
# In controller constructor, verify DI:
public AnalyticsController(IAnalyticsService analyticsService, ...)
{
    _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
}
```

### Monitor Event Processing

```bash
# Check logs for event tracking
grep -i "analytics" app.log | tail -50

# Check error events
SELECT * FROM analytics.analytics_events 
WHERE event_data LIKE '%error%' 
LIMIT 10;
```

---

## Performance Expectations

- **Single event tracking:** <50ms
- **Batch tracking (100 events):** <500ms
- **Conversion rate calculation (30 days):** <200ms
- **Top products query (1000 products):** <100ms
- **Dashboard generation:** <500ms

---

## Next Steps

1. ✅ Implement analytics API
2. ⏳ Integrate frontend event tracking
3. ⏳ Add product view events
4. ⏳ Add order completion events
5. ⏳ Create admin dashboard UI
6. ⏳ Set up monitoring/alerts
7. ⏳ Performance optimization (archive old data)
8. ⏳ Real-time dashboard updates (WebSockets)
