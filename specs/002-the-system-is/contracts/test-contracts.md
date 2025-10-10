# Test Contracts: SQL Server Database Support

**Feature**: SQL Server Database Support with Testing
**Branch**: `002-the-system-is`
**Date**: 2025-10-07

## Overview

This document defines the test contracts for validating SQL Server support. These contracts specify what must be tested and the expected behaviors for both database providers.

## Test Organization

### Test Projects

1. **FormDesigner.Tests.Unit** - No database dependencies
2. **FormDesigner.Tests.Integration** - Full stack integration tests
3. **FormDesigner.Tests.Contract** - API contract tests

### Provider-Specific Test Fixtures

#### SqlServerTestFixture

**Location**: `backend/tests/FormDesigner.Tests.Integration/SqlServerTestFixture.cs` (to be created)

**Contract**:
```csharp
public class SqlServerTestFixture : IDisposable
{
    public HttpClient Client { get; }
    public string DatabaseName { get; }

    // Setup: Configure app with SQL Server provider
    // Teardown: Clean up test database
}
```

**Responsibilities**:
- Configure WebApplicationFactory with SQL Server provider
- Create isolated test database
- Provide HttpClient for API testing
- Clean up test data after tests

#### PostgresTestFixture

**Location**: `backend/tests/FormDesigner.Tests.Integration/IntegrationTestFixture.cs` (existing)

**Contract**: Same as SqlServerTestFixture but configured for PostgreSQL

## Test Categories

### Category 1: Database Provider Configuration

**Purpose**: Verify application correctly detects and uses configured database provider

#### Test: TC-001-Provider-Detection-PostgreSQL

**Given**: appsettings.json configured with `Database:Provider = "Postgres"`
**When**: Application starts
**Then**:
- ApplicationDbContext uses Npgsql provider
- Database.ProviderName equals "Npgsql.EntityFrameworkCore.PostgreSQL"

#### Test: TC-002-Provider-Detection-SqlServer

**Given**: appsettings.json configured with `Database:Provider = "SqlServer"`
**When**: Application starts
**Then**:
- ApplicationDbContext uses SqlServer provider
- Database.ProviderName equals "Microsoft.EntityFrameworkCore.SqlServer"

#### Test: TC-003-Provider-Case-Insensitive

**Given**: appsettings.json configured with variations ("sqlserver", "SQLSERVER", "SqlServer", "mssql")
**When**: Application starts
**Then**: All variations correctly resolve to SQL Server provider

#### Test: TC-004-Invalid-Provider

**Given**: appsettings.json configured with unsupported provider "MySQL"
**When**: Application starts
**Then**: InvalidOperationException thrown with clear error message

### Category 2: Schema Validation

**Purpose**: Verify database schema is created correctly for each provider

#### Test: TC-101-Schema-Tables-Exist

**Given**: Fresh database with migrations applied
**When**: Query INFORMATION_SCHEMA.TABLES
**Then**:
- form_definitions table exists
- form_instances table exists
- temporary_states table exists

#### Test: TC-102-Schema-Columns-Correct

**Given**: Database with migrations applied
**When**: Query INFORMATION_SCHEMA.COLUMNS
**Then**:
- All expected columns exist for each table
- Column data types match provider-specific mappings
- NOT NULL constraints match expectations

**PostgreSQL Expected Types**:
- FormId: text
- DslJson: jsonb
- CreatedAt: timestamp with time zone

**SQL Server Expected Types**:
- FormId: nvarchar(100)
- DslJson: nvarchar(max)
- CreatedAt: datetime2

#### Test: TC-103-Schema-Indexes-Exist

**Given**: Database with migrations applied
**When**: Query index metadata
**Then**:
- All non-provider-specific indexes exist
- Primary key indexes exist
- Foreign key indexes exist

**Acceptable Differences**:
- GIN indexes only on PostgreSQL (expected)

#### Test: TC-104-Schema-Foreign-Keys

**Given**: Database with migrations applied
**When**: Query foreign key constraints
**Then**:
- form_instances.form_id → form_definitions.form_id (RESTRICT)
- temporary_states.instance_id → form_instances.instance_id (CASCADE)

### Category 3: CRUD Operations

**Purpose**: Verify basic create, read, update, delete operations work on both providers

#### Test: TC-201-Create-Form-Definition

**Given**: Valid form definition JSON payload
**When**: POST /api/forms
**Then**:
- HTTP 201 Created
- Form persisted in database
- FormId matches request
- CreatedAt timestamp set
- IsActive = true
- IsCommitted = false

**Data Contract**:
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

#### Test: TC-202-Read-Form-Definition

**Given**: Form exists in database
**When**: GET /api/forms/{formId}
**Then**:
- HTTP 200 OK
- Response matches stored data
- JSON deserialization correct

#### Test: TC-203-Update-Form-Definition

**Given**: Existing form in database
**When**: PUT /api/forms/{formId}
**Then**:
- HTTP 200 OK
- Form updated in database
- UpdatedAt timestamp updated
- Version preserved or incremented

#### Test: TC-204-Delete-Form-Definition

**Given**: Existing active form
**When**: DELETE /api/forms/{formId}
**Then**:
- HTTP 204 No Content
- IsActive set to false (soft delete)
- UpdatedAt timestamp updated
- Form not returned in GET requests

#### Test: TC-205-List-Forms

**Given**: Multiple forms in database (some active, some inactive)
**When**: GET /api/forms
**Then**:
- HTTP 200 OK
- Only active forms returned
- Ordered by CreatedAt DESC

### Category 4: JSON Serialization

**Purpose**: Verify JSON data is correctly stored and retrieved in both providers

#### Test: TC-301-Simple-JSON-Roundtrip

**Given**: Form with basic JSON structure
**When**: Create form → Retrieve form
**Then**:
- JSON structure identical
- Property names use snake_case
- Null values handled correctly

#### Test: TC-302-Complex-JSON-Roundtrip

**Given**: Form with nested objects, arrays, and special characters
**When**: Create form → Retrieve form
**Then**:
- All nesting preserved
- Arrays maintain order
- Special characters not corrupted

**Test Data**:
```json
{
  "form": {
    "metadata": {
      "tags": ["test", "complex"],
      "description": "Unicode: 你好, Emoji: 🎉, Quotes: \"quoted\", Newline: \n"
    }
  }
}
```

#### Test: TC-303-Large-JSON-Storage

**Given**: Form with large JSON payload (~500KB)
**When**: Create form → Retrieve form
**Then**:
- Storage succeeds
- Retrieval succeeds
- Data integrity maintained

**Performance Contract**:
- PostgreSQL: <100ms for storage
- SQL Server: <150ms for storage (acceptable 50% overhead for NVARCHAR(MAX))

#### Test: TC-304-JSON-Null-Handling

**Given**: Form with explicit null values and missing properties
**When**: Create form → Retrieve form
**Then**:
- Null values preserved
- Missing properties not added
- Serialization consistent with JsonIgnoreCondition.WhenWritingNull

### Category 5: Timestamp Handling

**Purpose**: Verify timestamp defaults and UTC handling

#### Test: TC-401-CreatedAt-Default-PostgreSQL

**Given**: PostgreSQL provider
**When**: Insert form without specifying CreatedAt
**Then**:
- CreatedAt set by database default (NOW())
- Value is TIMESTAMPTZ (UTC)
- Within 1 second of current time

#### Test: TC-402-CreatedAt-Default-SqlServer

**Given**: SQL Server provider
**When**: Insert form without specifying CreatedAt
**Then**:
- CreatedAt set by database default (SYSUTCDATETIME())
- Value is DATETIME2 (UTC)
- Within 1 second of current time

#### Test: TC-403-UpdatedAt-Manual-Set

**Given**: Existing form
**When**: Update form
**Then**:
- UpdatedAt set by application code (DateTime.UtcNow)
- Value is UTC
- UpdatedAt > CreatedAt

#### Test: TC-404-Timestamp-Ordering

**Given**: Multiple forms created in sequence
**When**: Query forms ordered by CreatedAt DESC
**Then**:
- Most recent form first
- Chronological order correct
- No timezone conversion issues

### Category 6: SQL Generation

**Purpose**: Verify SQL DDL generation produces valid syntax for both providers

#### Test: TC-501-Generate-SQL-For-PostgreSQL

**Given**: Form with table widget, PostgreSQL provider active
**When**: GET /api/export/{formId}/sql
**Then**:
- SQL contains PostgreSQL-specific types (VARCHAR, TIMESTAMPTZ, BOOLEAN)
- CREATE TABLE syntax valid for PostgreSQL
- Computed columns use STORED keyword

#### Test: TC-502-Generate-SQL-For-SqlServer

**Given**: Form with table widget, SQL Server provider active
**When**: GET /api/export/{formId}/sql
**Then**:
- SQL contains SQL Server-specific types (NVARCHAR, DATETIME2, BIT)
- CREATE TABLE syntax valid for SQL Server
- Computed columns use PERSISTED keyword

#### Test: TC-503-Type-Mapping-Correctness

**Given**: Table widget with all supported data types
**When**: Generate SQL DDL
**Then**:
- Each DSL type maps correctly to database type
- String → VARCHAR/NVARCHAR
- Integer → INTEGER/INT
- Decimal → NUMERIC(18,2)/DECIMAL(18,2)
- Bool → BOOLEAN/BIT
- DateTime → TIMESTAMPTZ/DATETIME2

#### Test: TC-504-Computed-Column-Syntax

**Given**: Table with computed column (formula: "quantity * unit_price")
**When**: Generate SQL DDL
**Then**:
- PostgreSQL: `GENERATED ALWAYS AS (...) STORED`
- SQL Server: `AS (...) PERSISTED`

### Category 7: Integration Test Parity

**Purpose**: Verify all existing integration tests pass on both providers

#### Test: TC-601-All-Integration-Tests-PostgreSQL

**Given**: PostgreSQL provider configured
**When**: dotnet test --filter "Category=Integration"
**Then**: 100% pass rate

#### Test: TC-602-All-Integration-Tests-SqlServer

**Given**: SQL Server provider configured
**When**: dotnet test --filter "Category=Integration"
**Then**: 100% pass rate

#### Test: TC-603-Contract-Tests-PostgreSQL

**Given**: PostgreSQL provider configured
**When**: dotnet test --filter "Category=Contract"
**Then**: 100% pass rate

#### Test: TC-604-Contract-Tests-SqlServer

**Given**: SQL Server provider configured
**When**: dotnet test --filter "Category=Contract"
**Then**: 100% pass rate

### Category 8: Error Handling

**Purpose**: Verify graceful error handling for database issues

#### Test: TC-701-Connection-Failure

**Given**: Invalid connection string
**When**: Application starts
**Then**:
- Clear error message logged
- Application fails to start (no silent failures)
- Error indicates connection issue

#### Test: TC-702-Database-Does-Not-Exist

**Given**: Connection string to non-existent database
**When**: Application starts
**Then**:
- Error message indicates database not found
- Suggests running migrations or setup script

#### Test: TC-703-Schema-Version-Mismatch

**Given**: Database schema outdated (missing migration)
**When**: Application starts
**Then**:
- Clear error about pending migrations
- Suggests running `dotnet ef database update`

### Category 9: Performance

**Purpose**: Verify performance is acceptable for both providers

#### Test: TC-801-Startup-Time-PostgreSQL

**Given**: PostgreSQL provider
**When**: Measure application startup time
**Then**: Record baseline time (e.g., 2.5 seconds)

#### Test: TC-802-Startup-Time-SqlServer

**Given**: SQL Server provider
**When**: Measure application startup time
**Then**: Within 10% of PostgreSQL baseline (per SC-004)

#### Test: TC-803-API-Response-Time-PostgreSQL

**Given**: PostgreSQL provider, 100 forms in database
**When**: GET /api/forms
**Then**: Record baseline response time (e.g., 50ms)

#### Test: TC-804-API-Response-Time-SqlServer

**Given**: SQL Server provider, 100 forms in database
**When**: GET /api/forms
**Then**: Within 15% of PostgreSQL baseline (per SC-005)

## Test Execution Matrix

| Test Category | PostgreSQL | SQL Server | Notes |
|---------------|-----------|------------|-------|
| Provider Config (TC-001-004) | ✅ | ✅ | |
| Schema Validation (TC-101-104) | ✅ | ✅ | |
| CRUD Operations (TC-201-205) | ✅ | ✅ | |
| JSON Serialization (TC-301-304) | ✅ | ✅ | |
| Timestamps (TC-401-404) | ✅ | ✅ | |
| SQL Generation (TC-501-504) | ✅ | ✅ | Provider-specific |
| Integration Parity (TC-601-604) | ✅ | ✅ | |
| Error Handling (TC-701-703) | ✅ | ✅ | |
| Performance (TC-801-804) | ✅ | ✅ | Comparison tests |

## Acceptance Criteria

For this feature to be considered complete:

1. ✅ All TC-xxx tests defined and implemented
2. ✅ 100% pass rate on PostgreSQL
3. ✅ 100% pass rate on SQL Server
4. ✅ Performance within acceptable thresholds
5. ✅ No known critical bugs
6. ✅ Documentation complete

## Test Data Location

Shared test data: `specs/002-the-system-is/test-data/`

Sample files:
- `simple-form.json` - Basic form for CRUD tests
- `complex-form.json` - Nested structure for JSON tests
- `table-form.json` - Form with table widget for SQL generation
- `large-form.json` - Large payload for performance tests

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: SQL Server Tests
on: [push, pull_request]

jobs:
  test-sqlserver:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2019-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong!Passw0rd
        ports:
          - 1433:1433
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Setup Database
        run: sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -i backend/database/setup.sqlserver.sql
      - name: Run Tests
        run: dotnet test
        env:
          Database__Provider: SqlServer
          ConnectionStrings__FormDesignerDb: "Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

## Contract Versioning

**Version**: 1.0
**Last Updated**: 2025-10-07
**Status**: Draft

**Change Log**:
- 2025-10-07: Initial contract definition

## References

- Feature Spec: `specs/002-the-system-is/spec.md`
- Data Model: `specs/002-the-system-is/data-model.md`
- Quickstart: `specs/002-the-system-is/quickstart.md`
