# EF Core Database Migration Instructions

## Creating the Migration

The `SearchIndexEventLog` entity requires a new database migration to create the `search_index_events` table.

### Step 1: Create Migration

From the project root:

```bash
cd TrendplusProdavnica.Infrastructure
dotnet ef migrations add AddSearchIndexEventLog
```

This will create a new migration file in the `Migrations/` folder with the name pattern:
`[Timestamp]_AddSearchIndexEventLog.cs`

### Step 2: Review Generated Migration

Check the generated migration file to verify it includes:
- Table `search_index_events` creation
- All columns and data types
- Indexes creation
- Constraints

Example structure:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "search_index_events",
        columns: table => new
        {
            id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            event_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
            type = table.Column<string>(type: "text", nullable: false),
            product_id = table.Column<long>(type: "bigint", nullable: false),
            // ... more columns ...
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_search_index_events", x => x.id);
        });

    // Create indexes
    migrationBuilder.CreateIndex(
        name: "ix_search_index_events_event_id",
        table: "search_index_events",
        column: "event_id",
        unique: true);
}
```

### Step 3: Apply Migration to Database

```bash
dotnet ef database update
```

This will:
1. Connect to your PostgreSQL database (using connection string from configuration)
2. Execute the migration
3. Create the `search_index_events` table with all indexes

### Step 4: Verify Migration

```bash
# Check if migration was applied (from project root)
dotnet ef migrations list

# You should see:
# [Timestamp] AddSearchIndexEventLog (Applied)
```

### (Optional) Check PostgreSQL Directly

```sql
-- Connect to your PostgreSQL database and verify table was created
\dt search_index_events;     -- List table

-- View table structure
\d+ search_index_events;     -- Detailed table info

-- Check indexes
SELECT indexname FROM pg_indexes 
WHERE tablename = 'search_index_events';

-- Count existing records
SELECT COUNT(*) FROM search_index_events;
```

## Reverting Migration (If Needed)

If you need to rollback this migration:

```bash
# Remove the last migration (only if not yet pushed to production)
dotnet ef migrations remove

# Or revert to previous migration
dotnet ef database update [PreviousMigrationName]
```

## Production Deployment

For production deployment:

1. **Backup your database first**
   ```bash
   pg_dump -U postgres -d trendplus_db -F c > backup_$(date +%Y%m%d).dump
   ```

2. **Apply migration in staging environment**
   ```bash
   dotnet ef database update --environment Staging
   ```

3. **Test event queue processing**
   - Verify workers are running
   - Check queue/DLQ status endpoints
   - Monitor logs for errors

4. **Apply to production**
   ```bash
   dotnet ef database update --environment Production
   ```

5. **Monitor after deployment**
   - Watch application logs
   - Monitor queue sizes
   - Check OpenSearch index updates

## Troubleshooting

### Migration Not Found
```bash
# Ensure you're in the correct project directory
cd TrendplusProdavnica.Infrastructure

# Check if migrations folder exists
ls -la Migrations/
```

### Connection String Not Found
```bash
# Make sure appsettings.json has correct connection string
cat appsettings.json | grep -A 3 "TrendplusDb"

# Or set via environment variable
export ConnectionStrings__TrendplusDb="Host=localhost;Port=5432;Database=trendplus_db;Username=postgres;Password=..."
dotnet ef database update
```

### Npgsql/PostgreSQL Provider Not Installed
```bash
cd TrendplusProdavnica.Infrastructure

# Add required packages if missing
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Restore packages
dotnet restore
```

### Table Already Exists
If you get "table already exists" error:
1. Check if `search_index_events` table exists in database
2. If it does, manually add the migration result or drop the table
3. Re-run migration

### Stuck in Pending
```bash
# Remove pending migrations
dotnet ef migrations remove

# Recreate
dotnet ef migrations add AddSearchIndexEventLog
```

## Monitoring Migrations

View all applied migrations:
```bash
dotnet ef migrations list
```

View migration details:
```bash
# Show raw SQL that will be executed
dotnet ef migrations script [FromMigration] [ToMigration]

# Example: show last migration SQL
dotnet ef migrations script
```

---

For more information, see:
- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Npgsql EF Core Documentation](https://www.npgsql.org/efcore/)

Version: 1.0
Updated: April 2026
