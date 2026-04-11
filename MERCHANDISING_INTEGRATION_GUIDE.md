# Merchandising Rules Integration Guide

## Overview
Ova dokumentacija objašnjava kako integrisati Merchandising Rules Engine sa ProductListingQueryService kako bi se osiguralo da se pravila primenjuju na proizvode pri listanju.

## Current Status

### ✅ Completed Components
- MerchandisingRule domain entity
- IMerchandisingService & MerchandisingService implementation
- MerchandisingRuleEvaluator algorithm
- MerchandisingRulesAdminController API
- Database migration (MerchandisingRules table)
- DI container registration
- API documentation

### ⏳ Pending: ProductListingQueryService Integration

## Architecture

```
ProductListingQueryService
    ↓
1. Fetch products from database
    ↓
2. Apply filters (category, brand, price range)
    ↓
3. [NEW] Evaluate Merchandising Rules
    ↓   ↓
    ├─→ GetActiveRules()
    ├─→ EvaluatRulesAsync(products)
    └─→ Apply boostScore adjustments
    ↓
4. Apply default sorting (popularity, rating, price, etc.)
    ↓
5. Paginate results
    ↓
Return ProductListingDto[]
```

## Integration Steps

### Step 1: Inject IMerchandisingService
In ProductListingQueryService constructor:
```csharp
private readonly IMerchandisingService _merchandisingService;

public ProductListingQueryService(
    TrendplusDbContext db,
    ILogger<ProductListingQueryService> logger,
    IMerchandisingService merchandisingService)  // Add this
{
    _db = db;
    _logger = logger;
    _merchandisingService = merchandisingService;
}
```

### Step 2: Retrieve Products
Existing logic remains the same - fetch products from database after filters.

### Step 3: Prepare Evaluation Input
Convert products to RuleEvaluationInput:
```csharp
var evaluationInputs = products.Select(p => new RuleEvaluationInput
{
    ProductId = p.Id,
    CategoryId = p.CategoryId,  // or from CategoryProductMap
    BrandId = p.BrandId,
    CurrentScore = CalculateCurrentScore(p)  // Your existing scoring logic
}).ToList();
```

### Step 4: Evaluate Rules
```csharp
var ruleAdjustments = await _merchandisingService.EvaluateRulesAsync(
    evaluationInputs,
    DateTimeOffset.UtcNow
);
```

### Step 5: Apply Adjustments & Sort
```csharp
var sortedProducts = products
    .Select(p => new
    {
        Product = p,
        AdjustedScore = ruleAdjustments.ContainsKey(p.Id) 
            ? ruleAdjustments[p.Id]
            : CalculateCurrentScore(p)
    })
    .OrderByDescending(x => x.AdjustedScore)
    .ThenByDescending(x => x.Product.Rating)
    .ThenBy(x => x.Product.Name)
    .Select(x => x.Product)
    .ToList();
```

## Implementation Example

Here's a complete integration pattern:

```csharp
public async Task<ProductListingResponse> GetListingAsync(
    ProductListingRequest request,
    CancellationToken cancellationToken)
{
    // 1. Apply basic filters
    var query = _db.Products
        .Where(p => p.IsActive);

    if (request.CategoryId.HasValue)
        query = query.Where(p => p.CategoryId == request.CategoryId.Value);

    if (request.BrandId.HasValue)
        query = query.Where(p => p.BrandId == request.BrandId.Value);

    // 2. Get baseline products
    var allProducts = await query
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    // 3. Prepare for merchandising evaluation
    var evaluationInputs = allProducts.Select(p => new RuleEvaluationInput
    {
        ProductId = p.Id,
        CategoryId = p.CategoryId,
        BrandId = p.BrandId,
        CurrentScore = CalculateBaseScore(p)  // rating + popularity + price relevance
    }).ToList();

    // 4. Evaluate merchandising rules
    var ruleAdjustments = await _merchandisingService.EvaluateRulesAsync(
        evaluationInputs,
        DateTimeOffset.UtcNow
    );

    // 5. Create result set with adjusted scoring
    var scoredProducts = allProducts
        .Select(p => new ProductListingItem
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Price = p.Price,
            Image = p.PrimaryImage,
            Rating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            // This is where merchandising rules effect the order
            _SortScore = ruleAdjustments.ContainsKey(p.Id)
                ? ruleAdjustments[p.Id]
                : CalculateBaseScore(p)
        })
        .OrderByDescending(x => x._SortScore)
        .ThenByDescending(x => x.Rating)
        .ThenBy(x => x.Name)
        .ToList();

    // 6. Apply pagination
    var pagedResults = scoredProducts
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToList();

    return new ProductListingResponse
    {
        Items = pagedResults,
        TotalCount = scoredProducts.Count,
        Page = request.Page,
        PageSize = request.PageSize
    };
}

private decimal CalculateBaseScore(Product product)
{
    // Your existing scoring logic
    var ratingScore = (product.AverageRating ?? 0) * 20;            // 0-100
    var popularityScore = product.ReviewCount * 0.5m;              // Count-based
    var priceRelevance = CalculatePriceRelevance(product.Price);   // 0-100
    
    return ratingScore + popularityScore + priceRelevance;
}
```

## Error Handling

```csharp
try
{
    var ruleAdjustments = await _merchandisingService.EvaluateRulesAsync(
        evaluationInputs,
        DateTimeOffset.UtcNow
    );

    // If evaluation fails, ruleAdjustments will be empty Dictionary<>
    // Products will fall back to default scoring
    if (!ruleAdjustments.Any())
    {
        _logger.LogWarning("Merchandising rules evaluation returned no adjustments");
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error evaluating merchandising rules");
    // Gracefully fall back to default sorting
    // Don't let merchandising errors break the listing
}
```

## Performance Optimization

### Caching Strategy
- Merchandising rules are cached for 30 minutes
- Cache is invalidated on Create/Update/Delete via admin API
- First request after cache invalidation may be slightly slower
- Subsequent requests use cached rules

### Database Query Optimization
```csharp
// Good: Batch evaluation
var ruleAdjustments = await _merchandisingService.EvaluateRulesAsync(
    allProducts.Select(p => new RuleEvaluationInput { ... }).ToList()
);

// Avoid: Per-product evaluation in loop
foreach (var product in allProducts)
{
    var adjustments = await _merchandisingService.EvaluateRulesAsync(
        new[] { ruleInput }  // ❌ Inefficient
    );
}
```

### Query Hints
- Use `AsNoTracking()` for read-only queries
- Filter products before evaluation when possible
- Consider pagination limits (don't evaluate millions of products)

## Testing

### Manual Test Endpoint
Create a test endpoint to verify merchandising logic:
```csharp
[HttpGet("debug/scoring")]
[AllowAnonymous]
public async Task<IActionResult> GetScoringDebug(long productId)
{
    var product = await _db.Products.FindAsync(productId);
    var baseScore = CalculateBaseScore(product);
    
    var adjustments = await _merchandisingService.EvaluateRulesAsync(
        new[] { new RuleEvaluationInput
        {
            ProductId = product.Id,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            CurrentScore = baseScore
        }}
    );

    return Ok(new
    {
        ProductId = productId,
        BaseScore = baseScore,
        AdjustedScore = adjustments.ContainsKey(productId) 
            ? adjustments[productId] 
            : baseScore,
        AppliedRules = await _merchandisingService.GetActiveRulesAsync()
    });
}
```

### Unit Test Example
```csharp
[Test]
public async Task PinRuleShouldHaveHighestScore()
{
    // Arrange
    var pinRule = new MerchandisingRule(
        "Test Pin", 
        MerchandisingRuleType.Pin, 
        0, 
        1
    )
    {
        ProductId = 123,
        IsActive = true,
        StartDateUtc = DateTimeOffset.Now.AddDays(-1),
        EndDateUtc = DateTimeOffset.Now.AddDays(1)
    };

    var evaluator = new MerchandisingRuleEvaluator();
    var input = new RuleEvaluationInput
    {
        ProductId = 123,
        CategoryId = 5,
        BrandId = 10,
        CurrentScore = 100
    };

    // Act
    var result = evaluator.Evaluate(new[] { input }, new[] { pinRule }, DateTimeOffset.Now);

    // Assert
    Assert.That(result[123], Is.GreaterThan(10000));
}
```

## Monitoring & Logging

Key logging points:
```csharp
_logger.LogInformation(
    "Evaluated merchandising rules for {ProductCount} products",
    evaluationInputs.Count);

_logger.LogDebug(
    "Merchandising rule {RuleId} applied to product {ProductId}",
    rule.Id, product.Id);

_logger.LogWarning(
    "Merchandising rules evaluation took {Duration}ms",
    sw.ElapsedMilliseconds);
```

## Rollback Plan

If merchandising rules cause issues:
1. Deactivate all rules: `UPDATE merchandising_rules SET IsActive = false`
2. Invalidate cache: `POST /api/admin/merchandising/cache/invalidate`
3. ProductListingQueryService will fall back to default scoring
4. No database downtime required

## Future Enhancements

### Potential Improvements
- A/B testing: Compare conversion rates with/without specific rules
- Rules scheduling: Automatic activation/deactivation
- Bulk rule management: Import CSV of rules
- Analytics: Track which rules drive conversions
- Dynamic evaluation: Adjust scores based on inventory levels
- Seasonal templates: Pre-built rule sets for holidays
- Audience targeting: Rules based on user segment/geography

## Integration Checklist

- [ ] Add IMerchandisingService DI injection to ProductListingQueryService
- [ ] Implement RuleEvaluationInput conversion
- [ ] Call EvaluateRulesAsync() in GetListingAsync()
- [ ] Apply ruleAdjustments to scoring logic
- [ ] Handle empty/null ruleAdjustments gracefully
- [ ] Add error logging for evaluation failures
- [ ] Create debug endpoint for testing
- [ ] Write unit tests for evaluation logic
- [ ] Load test with typical data volumes
- [ ] Update API documentation
- [ ] Create admin UI for rules management
- [ ] Set up monitoring/alerts
- [ ] Deploy to staging
- [ ] A/B test before full rollout
