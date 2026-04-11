# OpenSearch Sync Pipeline Implementation

## Implementacija

OpenSearch sync pipeline sa event-driven architecture-om, retry logic-om i dead letter queue (DLQ) support-om.

## Arhitektura

```
PostgreSQL (Source of Truth)
    ↓
[Event Triggered]
    ↓
[ProductSearchIndexer - In-Memory Queue]
    ├─→ [Retry Logic with Exponential Backoff]
    └─→ [Dead Letter Queue on Max Retries]
    ↓
[ProductSearchIndexSyncWorker - Background Service]
    ├─→ Process Queue (every 10 seconds)
    ├─→ Retry DLQ (every 100 seconds)
    └─→ Update OpenSearch Index
```

## Komponente

### 1. SearchIndexEventLog (Domain Entity)

EF Core entity za tracking-ovanje svih search index events-a i DLQ entries.

**Fields:**
- `EventId` - Unique event identifier (GUID)
- `Type` - Event type (ProductCreated, ProductUpdated, ProductDeleted, itd.)
- `ProductId` - Product being indexed
- `CreatedAtUtc` - Kada je event kreiran
- `RetryCount` - Koliko puta je pokušan retry
- `LastErrorMessage` - Poslednja error poruka
- `LastRetryAtUtc` - Vreme poslednjeg retry-a
- `IsProcessed` - Da li je event uspešno obradjen
- `IsDeadLettered` - Da li je u DLQ-u
- `DeadLetteredAtUtc` - Kada je prebačen u DLQ
- `DeadLetterReason` - Razlog prelaska u DLQ
- `ProcessedAtUtc` - Kada je event obrađen

**Database Table:** `search_index_events`

**Indexes:**
- Primary: `id`
- Unique: `event_id`
- Composite: `(is_processed, created_at_utc)` - queries za pending events
- Composite: `(is_dead_lettered, dead_lettered_at_utc)` - queries za DLQ
- Simple: `product_id` - product tracking

### 2. ProductSearchIndexer (Service)

Event-driven indexer servis sa queue management-om.

**Methods:**

```csharp
// Queue a single product for indexing
Task QueueProductAsync(long productId, SearchIndexEventType eventType)

// Queue multiple products
Task QueueProductsAsync(IEnumerable<long> productIds, SearchIndexEventType eventType)

// Process all pending events in queue
Task ProcessQueueAsync()

// Get current queue size
Task<int> GetQueueSizeAsync()

// Get dead letter queue size
Task<int> GetDeadLetterQueueSizeAsync()

// Retry failed events from DLQ
Task RetryDeadLetterAsync(int maxAttempts = 10)
```

**Event Types:**

```csharp
public enum SearchIndexEventType
{
    ProductCreated,      // Novi proizvod
    ProductUpdated,      // Proizvod ažuriran
    ProductPriceChanged, // Cena promenjena
    InventoryChanged,    // Stanje zaliha promenjeno
    ProductDeleted,      // Proizvod obrisan
    FullReindex         // Potpuni reindex svih proizvoda
}
```

### 3. ProductSearchIndexSyncWorker (Background Service)

HostedService koji periodički obrađuje queue i DLQ events.

**Konfiguracija:**

```json
{
  "SearchIndexingSyncWorker": {
    "InitialDelayMs": 5000,           // Čekanje pre prvog pokretanja
    "IntervalMs": 10000,              // Interval između iteracija (10 sekundi)
    "MaxDLQRetryAttempts": 10,        // Maks eventos za DLQ retry
    "Enabled": true                   // Uključi/isključi worker
  }
}
```

**Proces:**
1. Čeka InitialDelayMs na startup
2. Svakog IntervalMs:
   - Obradi sve pending events iz queue-a
   - Svakih 10 iteracija, pokušaj recovery DLQ events-a

### 4. Retry Logic with Exponential Backoff

**Konfiguracija:**

```csharp
public sealed class SearchIndexEventConfig
{
    public int MaxRetries { get; set; } = 3;                          // Max retry attempts
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);     // 5s
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(60);        // 60s
    public double RetryBackoffMultiplier { get; set; } = 2.0;         // Exponential backoff
}
```

**Retry Schedule (sa default konfiguracijom):**

```
Attempt 1: Fail → Queue for retry
Attempt 2: 5s later → Fail → Queue for retry (delay = 5 * 2^1 = 10s)
Attempt 3: 10s later → Fail → Queue for retry (delay = 5 * 2^2 = 20s)
Attempt 4: 20s later → Fail → Move to DLQ (max retries exceeded)
```

### 5. Dead Letter Queue (DLQ)

Events koji fail-uju nakon maxRetries se prebacuju u DLQ.

**DLQ Status:**
- `IsDeadLettered = true`
- `DeadLetteredAtUtc` - Timestamp
- `DeadLetterReason` - Razlog failure-a

**Recovery:**
- Automatski attempts svakih ~100 iteracija worker-a
- Ručno putem `/api/admin/search/dlq/retry` endpoint-a

## API Endpoints

### Queue Management

#### 1. Process Queue
```
POST /api/admin/search/queue/process

Response:
{
  "status": "ok",
  "message": "Search index queue processed.",
  "remainingQueueSize": 0
}
```

#### 2. Get Queue Status
```
GET /api/admin/search/queue/status

Response:
{
  "queueSize": 42,
  "deadLetterQueueSize": 3,
  "timestamp": "2026-04-09T10:30:00Z"
}
```

#### 3. Get DLQ Status
```
GET /api/admin/search/dlq/status

Response:
{
  "deadLetterQueueSize": 3,
  "message": "Found dead letter queue events. Use /api/admin/search/dlq/retry to recover.",
  "timestamp": "2026-04-09T10:30:00Z"
}
```

#### 4. Retry DLQ Events
```
POST /api/admin/search/dlq/retry

Response:
{
  "status": "ok",
  "message": "Attempted to recover 3 dead letter queue events.",
  "recovered": 2,
  "remaining": 1,
  "timestamp": "2026-04-09T10:30:00Z"
}
```

### Reindex Endpoints (Existing)

#### Full Reindex
```
POST /api/admin/search/reindex

Response:
{
  "status": "ok",
  "message": "Full product reindex completed."
}
```

#### Single Product Reindex
```
POST /api/admin/search/reindex/{productId}

Response:
{
  "status": "ok",
  "message": "Product 123 reindexed."
}
```

## Setup & Configuration

### 1. Add to appsettings.json

```json
{
  "SearchIndexing": {
    "MaxRetries": 3,
    "InitialRetryDelayMs": 5000,
    "MaxRetryDelayMs": 60000,
    "RetryBackoffMultiplier": 2.0,
    "DeadLetterQueueMaxSize": 1000
  },
  "SearchIndexingSyncWorker": {
    "InitialDelayMs": 5000,
    "IntervalMs": 10000,
    "MaxDLQRetryAttempts": 10,
    "Enabled": true
  }
}
```

### 2. Create Database Migration

```bash
cd TrendplusProdavnica.Infrastructure
dotnet ef migrations add AddSearchIndexEventLog
dotnet ef database update
```

Migration će kreiati tabelu `search_index_events` sa svim potrebnim indexima.

### 3. Services Already Registered

U `InfrastructureServiceCollectionExtensions.cs` već su registrovani:
- `IProductSearchIndexer` → `ProductSearchIndexer`
- `ProductSearchIndexSyncWorker` (HostedService)
- `SearchIndexEventConfig` (configuration)
- `SearchIndexSyncWorkerConfig` (configuration)

## Event Triggering

### From Product Services

Kada se proizvod kreira, ažurira ili briše, trebate pozvati indexer:

```csharp
public class ProductAdminService
{
    private readonly IProductSearchIndexer _searchIndexer;

    public async Task CreateProductAsync(CreateProductRequest request)
    {
        // ... create product logic ...
        
        // Queue for search indexing
        await _searchIndexer.QueueProductAsync(
            product.Id, 
            SearchIndexEventType.ProductCreated);
    }

    public async Task UpdateProductAsync(long productId, UpdateProductRequest request)
    {
        // ... update product logic ...
        
        // Queue for search indexing
        await _searchIndexer.QueueProductAsync(
            productId, 
            SearchIndexEventType.ProductUpdated);
    }

    public async Task UpdateProductPriceAsync(long productId, decimal newPrice)
    {
        // ... update price logic ...
        
        // Queue for search indexing
        await _searchIndexer.QueueProductAsync(
            productId, 
            SearchIndexEventType.ProductPriceChanged);
    }

    public async Task DeleteProductAsync(long productId)
    {
        // ... delete product logic ...
        
        // Queue for search indexing
        await _searchIndexer.QueueProductAsync(
            productId, 
            SearchIndexEventType.ProductDeleted);
    }
}
```

### From Inventory Services

```csharp
public class InventoryService
{
    private readonly IProductSearchIndexer _searchIndexer;

    public async Task UpdateInventoryAsync(long productId, int quantity)
    {
        // ... update inventory ...
        
        // Queue for reindexing due to inventory change
        await _searchIndexer.QueueProductAsync(
            productId, 
            SearchIndexEventType.InventoryChanged);
    }
}
```

## Monitoring & Debugging

### check Queue Status

```bash
# Check queue size
curl -X GET https://localhost:7001/api/admin/search/queue/status

# Check DLQ status
curl -X GET https://localhost:7001/api/admin/search/dlq/status
```

### Process Queue Manually

```bash
# Process queue immediately (instead of waiting for worker)
curl -X POST https://localhost:7001/api/admin/search/queue/process
```

### Retry Dead Letter Events

```bash
# Attempt to recover DLQ events
curl -X POST https://localhost:7001/api/admin/search/dlq/retry
```

### Database Queries for Debugging

```sql
-- Check pending events
SELECT COUNT(*) FROM search_index_events 
WHERE is_processed = false AND is_dead_lettered = false;

-- Check dead lettered events
SELECT * FROM search_index_events 
WHERE is_dead_lettered = true
ORDER BY dead_lettered_at_utc DESC;

-- Check failed product indexing
SELECT product_id, COUNT(*) as event_count 
FROM search_index_events 
WHERE is_dead_lettered = true
GROUP BY product_id;

-- Clean up old processed events (optional)
DELETE FROM search_index_events 
WHERE is_processed = true 
AND processed_at_utc < NOW() - INTERVAL '30 days';
```

## Bulk Indexing

Svi batch operations koriste OpenSearch bulk API automatski:

```csharp
// produkta True
await _indexService.ReindexAllAsync();      // Bulk indexes sve proizvode

// Jednostavni proizvod
await _indexService.ReindexProductAsync(productId);  // Jednostavni index
```

## Performance Considerations

1. **Queue Processing**: 100 events po batch, svakih 10 sekundi
2. **DLQ Retry**: Svakih ~100s
3. **Exponential Backoff**: Sprečava thundering herd na failed events
4. **Database Indexes**: Optimizovani za pending/DLQ queries
5. **OpenSearch Bulk**: Koristi bulk API za bolju throughput

## Error Handling

1. **Transient Errors**: Automatski retry sa exponential backoff
2. **Permanent Errors**: Prebaceni u DLQ nakon max retries
3. **Logging**: Svi events logirani sa relevantnim detaljima
4. **Monitoring**: Queue/DLQ status dostupni kroz endpoints

---

Verzija: 1.0
Ažurirao: April 2026
