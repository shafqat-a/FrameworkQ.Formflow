# Data Model: SQL Server Database Support

**Feature**: SQL Server Database Support with Testing
**Branch**: `002-the-system-is`
**Date**: 2025-10-07

## Overview

This feature does NOT introduce new data models or entities. It validates and tests the existing data model against SQL Server, ensuring schema parity and correct behavior across both PostgreSQL and SQL Server providers.

## Existing Entities

### 1. FormDefinitionEntity

**Purpose**: Stores form definitions in both draft and committed states

**Location**: `backend/src/FormDesigner.API/Models/Entities/FormDefinitionEntity.cs`

| Property | Type | PostgreSQL Type | SQL Server Type | Constraints |
|----------|------|----------------|----------------|-------------|
| FormId | string | TEXT | NVARCHAR(100) | PK, NOT NULL, max 100 chars |
| Version | string | TEXT | NVARCHAR(50) | NOT NULL, max 50 chars |
| DslJson | string | JSONB | NVARCHAR(MAX) | NOT NULL |
| IsCommitted | bool | BOOLEAN | BIT | NOT NULL, default false |
| CreatedAt | DateTime | TIMESTAMPTZ | DATETIME2 | NOT NULL, default NOW()/SYSUTCDATETIME() |
| UpdatedAt | DateTime? | TIMESTAMPTZ | DATETIME2 | Nullable |
| IsActive | bool | BOOLEAN | BIT | NOT NULL, default true |

**Indexes**:
- Primary Key: form_id
- ix_form_definitions_is_active (is_active)
- ix_form_definitions_is_committed (is_committed)
- ix_form_definitions_created_at (created_at DESC)
- ix_form_definitions_dsl_json (JSONB GIN index - PostgreSQL only)

### 2. FormInstanceEntity

**Purpose**: Stores runtime form submissions and draft states

**Location**: `backend/src/FormDesigner.API/Models/Entities/FormInstanceEntity.cs`

| Property | Type | PostgreSQL Type | SQL Server Type | Constraints |
|----------|------|----------------|----------------|-------------|
| InstanceId | Guid | UUID | UNIQUEIDENTIFIER | PK, NOT NULL |
| FormId | string | TEXT | NVARCHAR(100) | FK to FormDefinitionEntity, NOT NULL |
| Status | string | TEXT | NVARCHAR(20) | NOT NULL, default 'draft', max 20 chars |
| DataJson | string? | JSONB | NVARCHAR(MAX) | Nullable |
| CreatedAt | DateTime | TIMESTAMPTZ | DATETIME2 | NOT NULL, default NOW()/SYSUTCDATETIME() |
| SubmittedAt | DateTime? | TIMESTAMPTZ | DATETIME2 | Nullable |
| UserId | string? | TEXT | NVARCHAR(256) | Nullable, max 256 chars |

**Relationships**:
- Foreign Key: form_id → form_definitions.form_id (ON DELETE RESTRICT)

**Indexes**:
- Primary Key: instance_id
- ix_form_instances_form_id (form_id)
- ix_form_instances_status (status)
- ix_form_instances_user_id (user_id)
- ix_form_instances_created_at (created_at DESC)
- ix_form_instances_data_json (JSONB GIN index - PostgreSQL only)

### 3. TemporaryStateEntity

**Purpose**: Stores in-progress form data for save/resume functionality

**Location**: `backend/src/FormDesigner.API/Models/Entities/TemporaryStateEntity.cs`

| Property | Type | PostgreSQL Type | SQL Server Type | Constraints |
|----------|------|----------------|----------------|-------------|
| StateId | Guid | UUID | UNIQUEIDENTIFIER | PK, NOT NULL |
| InstanceId | Guid | UUID | UNIQUEIDENTIFIER | FK to FormInstanceEntity, NOT NULL |
| DataJson | string | JSONB | NVARCHAR(MAX) | NOT NULL |
| SavedAt | DateTime | TIMESTAMPTZ | DATETIME2 | NOT NULL, default NOW()/SYSUTCDATETIME() |
| UserId | string? | TEXT | NVARCHAR(256) | Nullable, max 256 chars |

**Relationships**:
- Foreign Key: instance_id → form_instances.instance_id (ON DELETE CASCADE)

**Indexes**:
- Primary Key: state_id
- ix_temporary_states_instance_id (instance_id)
- ix_temporary_states_saved_at (saved_at DESC)
- ix_temporary_states_user_id (user_id)

## Database-Specific Configuration

### JSON Storage Strategy

**PostgreSQL**:
```csharp
propertyBuilder.HasColumnType("jsonb");
```
- Native JSONB support
- Queryable with @>, ->, ->> operators
- GIN indexes for performance

**SQL Server**:
```csharp
propertyBuilder.HasColumnType("nvarchar(max)");
```
- JSON stored as text
- SQL Server 2016+ has JSON functions (JSON_VALUE, JSON_QUERY)
- Full-text indexes possible, but not using GIN

### Timestamp Defaults

**PostgreSQL**:
```csharp
propertyBuilder.HasDefaultValueSql("NOW()");
```
- Returns TIMESTAMPTZ (timestamp with timezone)
- UTC by default

**SQL Server**:
```csharp
propertyBuilder.HasDefaultValueSql("SYSUTCDATETIME()");
```
- Returns DATETIME2
- Explicitly UTC (SYSUTCDATETIME vs SYSDATETIME)

## Schema Validation Requirements

### Column Type Equivalence

The following type mappings must be validated in tests:

| EF Core Type | PostgreSQL | SQL Server | Notes |
|--------------|-----------|------------|-------|
| string (100) | TEXT | NVARCHAR(100) | Max length enforced |
| string (max) | TEXT | NVARCHAR(MAX) | Unlimited length |
| bool | BOOLEAN | BIT | True/False vs 1/0 |
| DateTime | TIMESTAMPTZ | DATETIME2 | Both store UTC |
| Guid | UUID | UNIQUEIDENTIFIER | 16-byte identifier |

### Constraint Parity

Both providers must enforce:
- Primary key constraints
- Foreign key constraints
- NOT NULL constraints
- Default value constraints
- Check constraints (if any)

### Index Parity

All indexes must exist on both providers except:
- GIN indexes (PostgreSQL only - acceptable difference)

## Data Serialization

### JSON Serialization Settings

**Configuration**: `Program.cs:16-21`

```csharp
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.WriteIndented = false;
});
```

**Must Validate**:
- JSON round-trip (serialize → store → retrieve → deserialize)
- Snake_case property naming preserved
- Null handling consistent
- Complex nested objects work correctly
- Special characters in JSON strings escaped properly

### Known Limitations

**SQL Server NVARCHAR(MAX) Limits**:
- Maximum 2GB storage
- Performance degrades with very large JSON (>1MB)
- No native indexing of JSON content (unlike PostgreSQL GIN)

**Test Requirement**: Validate behavior when JSON approaches limits

## Migration Strategy

### Current Migrations

**Location**: `backend/src/FormDesigner.API/Data/Migrations/`

- `20251001214159_InitialCreate.cs` - Initial schema
- `ApplicationDbContextModelSnapshot.cs` - Current model state

### Migration Testing Requirements

1. **Apply Migrations to SQL Server**
   - Verify `dotnet ef migrations add` generates SQL Server-compatible code
   - Test `dotnet ef database update` succeeds against SQL Server
   - Validate schema matches manual setup.sqlserver.sql

2. **Migration Idempotency**
   - Running migrations multiple times should not error
   - Downgrade/upgrade cycles should work

3. **Data Preservation**
   - If adding new migrations, existing data must not be lost
   - Migration must handle both providers

### Provider-Specific Migrations (If Needed)

If migrations diverge between providers:
- Use `MigrationBuilder.IsSqlServer()` / `IsNpgsql()` extensions
- Document differences in migration comments
- Test both paths independently

## Testing Data Model

### Test Fixtures Required

1. **Schema Validation Fixture**
   - Verify all tables exist
   - Verify all columns exist with correct types
   - Verify all indexes exist
   - Verify all constraints exist

2. **Data Persistence Fixture**
   - Insert test data
   - Retrieve and compare
   - Update and verify
   - Delete (soft) and verify

3. **JSON Handling Fixture**
   - Store complex JSON structures
   - Query by JSON properties (if supported)
   - Verify serialization/deserialization

### Test Data Examples

**Simple Form Definition**:
```json
{
  "form": {
    "id": "test-form",
    "title": "Test Form",
    "version": "1.0",
    "pages": [...]
  }
}
```

**Large Form Definition** (edge case):
- 10+ pages
- 50+ fields
- Nested widgets
- Test JSON size limits

**Special Characters in JSON**:
- Unicode characters
- Escaped quotes
- Newlines and tabs
- Emoji (if supported)

## Schema Diff Detection

### Automated Comparison

Create utility to compare schemas:
1. Query PostgreSQL INFORMATION_SCHEMA
2. Query SQL Server INFORMATION_SCHEMA
3. Normalize types (TEXT → NVARCHAR, etc.)
4. Report differences

### Acceptable Differences

- JSONB (PostgreSQL) vs NVARCHAR(MAX) (SQL Server)
- TIMESTAMPTZ (PostgreSQL) vs DATETIME2 (SQL Server)
- GIN indexes (PostgreSQL only)
- Database-specific default value SQL

### Unacceptable Differences

- Missing columns
- Missing tables
- Missing indexes (except GIN)
- Missing foreign keys
- Different NOT NULL constraints
- Different primary keys

## Performance Considerations

### PostgreSQL Advantages
- Native JSONB with GIN indexes
- JSONB operators for querying

### SQL Server Advantages
- JSON functions (JSON_VALUE, JSON_QUERY) in SQL Server 2016+
- Potential better performance on Windows infrastructure

### Test Performance Metrics

Per spec SC-004 and SC-005:
- Startup time difference < 10%
- API response time difference < 15%

Measure:
1. Cold start time
2. First query time
3. Bulk insert time (100 forms)
4. JSON query time (if using JSON operators)

## Rollback Strategy

If SQL Server implementation has critical issues:
1. Configuration allows instant rollback to PostgreSQL
2. No data migration needed (different databases)
3. Schema scripts available for re-provisioning

## Data Model Summary

**No new entities introduced** - this feature validates existing schema across providers.

**Key Validation Points**:
- ✅ Schema parity (tables, columns, indexes)
- ✅ Type mapping correctness
- ✅ Constraint enforcement
- ✅ JSON serialization/deserialization
- ✅ Timestamp handling
- ✅ Foreign key relationships
- ✅ Migration compatibility

**Schema Files**:
- PostgreSQL: `backend/database/setup.sql`
- SQL Server: `backend/database/setup.sqlserver.sql`
- EF Migrations: `backend/src/FormDesigner.API/Data/Migrations/`

**Next**: See `quickstart.md` for testing setup and `tasks.md` for implementation steps.
