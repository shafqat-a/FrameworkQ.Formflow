# SQL Server Migration Validation Guide

**Task**: T013 - Validate Migrations on SQL Server
**GitHub Issue**: [#30](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/30)

## Overview

This guide documents how to validate EF Core migrations work correctly on SQL Server and verify schema consistency.

## Prerequisites

- SQL Server 2019+ running (local or Docker)
- .NET SDK 8.0+
- EF Core tools installed: `dotnet tool install --global dotnet-ef`

## Step-by-Step Validation

### 1. Start SQL Server

```bash
# Docker approach
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver-migrations \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Wait for SQL Server to be ready
sleep 10
```

### 2. Configure Environment

```bash
export Database__Provider=SqlServer
export ConnectionStrings__FormDesignerDb="Server=localhost,1433;Database=formflow_migrations_test;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

### 3. Create Database

```bash
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "CREATE DATABASE formflow_migrations_test"
```

### 4. Apply Migrations

```bash
cd backend/src/FormDesigner.API
dotnet ef database update --verbose
```

**Expected Output:**
```
Applying migration '20XX..._InitialCreate'.
Applying migration '20XX..._AddFormInstances'.
...
Done.
```

### 5. Verify Schema

```sql
-- Check tables exist
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Expected tables:
-- - form_definitions
-- - form_instances
-- - temporary_states
-- - __EFMigrationsHistory

-- Check columns for form_definitions
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'form_definitions'
ORDER BY ORDINAL_POSITION;

-- Expected columns:
-- - form_id (nvarchar, 100, NO)
-- - version (nvarchar, 50, NO)
-- - dsl_json (nvarchar, -1, NO)
-- - is_committed (bit, NULL, NO)
-- - created_at (datetime2, NULL, NO)
-- - updated_at (datetime2, NULL, YES)
-- - is_active (bit, NULL, NO)
```

### 6. Compare with Manual Script

```bash
# Apply manual script to a fresh database
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "CREATE DATABASE formflow_manual_test"

sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -d formflow_manual_test \
  -i backend/database/setup.sqlserver.sql

# Compare schemas
# Use a schema comparison tool or manual inspection
```

### 7. Test Idempotency

```bash
# Run migrations again - should be no-op
cd backend/src/FormDesigner.API
dotnet ef database update --verbose

# Expected: "No migrations were applied. The database is already up to date."
```

### 8. Verify Migrations History

```sql
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;
```

## Validation Checklist

- [ ] All three tables created (form_definitions, form_instances, temporary_states)
- [ ] Column types match SQL Server mappings (NVARCHAR, DATETIME2, BIT, UNIQUEIDENTIFIER)
- [ ] NOT NULL constraints correct
- [ ] Default values configured (is_committed=0, is_active=1, created_at=SYSUTCDATETIME())
- [ ] Primary keys defined
- [ ] Foreign keys created with correct delete behavior (RESTRICT/CASCADE)
- [ ] Indexes created (excluding PostgreSQL-specific GIN indexes)
- [ ] Schema matches manual setup.sqlserver.sql
- [ ] Running migrations again is idempotent (no errors)
- [ ] __EFMigrationsHistory table tracks applied migrations

## Known Differences

### Migrations vs Manual Script

**Acceptable differences:**
1. **Index names**: EF Core may generate different index names than manual script
2. **Constraint names**: Auto-generated names may differ
3. **Column order**: May vary between migrations and manual script

**Must match:**
1. Table names
2. Column names and types
3. NOT NULL constraints
4. Default values
5. Foreign key relationships
6. Primary keys

## Troubleshooting

### Migration Fails with "Database does not exist"

```bash
# Ensure database is created first
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "CREATE DATABASE formflow_migrations_test"
```

### Migration Fails with Connection Error

```bash
# Test connection
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION"

# Check Docker container
docker ps | grep sqlserver
docker logs sqlserver-migrations
```

### Migration Creates Wrong Types

- Verify `ApplicationDbContext.cs` has provider detection logic
- Check that `Database.ProviderName` returns "Microsoft.EntityFrameworkCore.SqlServer"
- Ensure ConfigureJsonColumn and ConfigureUtcNowDefault methods work correctly

### Schema Comparison Shows Differences

```sql
-- Export schema from migrations database
-- Compare with setup.sqlserver.sql
-- Document any intentional vs unintentional differences
```

## Success Criteria

**T013 is complete when:**
- ✅ Fresh database + migrations produces working schema
- ✅ All tables, columns, indexes, foreign keys present
- ✅ Data types match SQL Server conventions
- ✅ Default values work correctly
- ✅ Schema functionally equivalent to manual script
- ✅ Migrations are idempotent
- ✅ No errors or warnings during migration

## Automated Test (Future Enhancement)

```csharp
[Fact]
public async Task ValidateMigrations_ShouldCreateCorrectSchema()
{
    // Create fresh database
    // Apply migrations
    // Query schema
    // Assert schema correctness
    // Drop database
}
```

This could be added to `SchemaValidationTests.cs` once SQL Server is available in CI/CD.

## References

- [Data Model](../../../specs/002-the-system-is/data-model.md) - Expected schema
- [Test Contracts](../../../specs/002-the-system-is/contracts/test-contracts.md) - TC-703
- [ApplicationDbContext.cs](../../../backend/src/FormDesigner.API/Data/ApplicationDbContext.cs) - Provider configuration
- [setup.sqlserver.sql](../../../backend/database/setup.sqlserver.sql) - Manual script for comparison

## Validation Status

- ⏳ Awaiting SQL Server instance for actual validation
- ✅ Documentation complete
- ✅ Validation procedure defined
- ✅ Success criteria documented
