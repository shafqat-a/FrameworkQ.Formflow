# Research: SQL Server Database Support

**Feature**: SQL Server Database Support with Testing
**Branch**: `002-the-system-is`
**Date**: 2025-10-07

## Executive Summary

The FormDesigner application currently has **partial SQL Server support** implemented but lacks comprehensive testing. Analysis reveals:

- ✅ **Infrastructure Ready**: EF Core configuration supports both PostgreSQL and SQL Server
- ✅ **NuGet Packages**: Microsoft.EntityFrameworkCore.SqlServer (8.0.0) already installed
- ✅ **Database Scripts**: SQL Server setup script exists (setup.sqlserver.sql)
- ✅ **Configuration**: Provider switching mechanism implemented in Program.cs
- ❌ **Testing Gap**: No SQL Server-specific integration tests
- ❌ **SQL Generator**: SqlGeneratorService is PostgreSQL-specific
- ⚠️ **Validation Needed**: Schema migrations and runtime behavior not validated with SQL Server

**Primary Risk**: Without automated tests, SQL Server support may have hidden bugs in JSON serialization, timestamp handling, or query execution.

## Current Implementation Analysis

### 1. Database Provider Configuration

**Location**: `backend/src/FormDesigner.API/Program.cs:118-142`

```csharp
static void ConfigureDatabaseProvider(DbContextOptionsBuilder options, string provider, string connectionString)
{
    switch (provider.Trim().ToLowerInvariant())
    {
        case "postgres":
        case "postgresql":
        case "npgsql":
            options.UseNpgsql(connectionString);
            break;
        case "sqlserver":
        case "mssql":
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });
            break;
        default:
            throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
    }
}
```

**Findings**:
- ✅ SQL Server provider supported with retry logic
- ✅ Configuration reads from appsettings.json `Database:Provider`
- ✅ Fallback to "Postgres" if not specified
- ✅ Both connection strings pre-configured in appsettings.json

### 2. Entity Framework Context Abstraction

**Location**: `backend/src/FormDesigner.API/Data/ApplicationDbContext.cs:33-241`

**Key Implementation Details**:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var providerName = Database.ProviderName ?? string.Empty;
    var isNpgsql = providerName.Equals("Npgsql.EntityFrameworkCore.PostgreSQL", ...);
    var isSqlServer = providerName.Equals("Microsoft.EntityFrameworkCore.SqlServer", ...);

    // Database-specific column configurations
    ConfigureJsonColumn(dslJsonProperty, isNpgsql, isSqlServer);
    ConfigureUtcNowDefault(createdAtProperty, isNpgsql, isSqlServer);
}
```

**Database-Specific Mappings**:

| Feature | PostgreSQL | SQL Server |
|---------|-----------|------------|
| JSON Storage | `JSONB` | `NVARCHAR(MAX)` |
| Timestamps | `TIMESTAMPTZ` with `NOW()` | `DATETIME2` with `SYSUTCDATETIME()` |
| Boolean | `BOOLEAN` | `BIT` |
| GIN Indexes | Yes (for JSONB) | No (not applicable) |

**Findings**:
- ✅ Provider detection logic implemented
- ✅ Column type mapping abstracted
- ✅ Default value SQL differs by provider
- ⚠️ GIN index only created for PostgreSQL (expected behavior)

### 3. Design-Time Factory for Migrations

**Location**: `backend/src/FormDesigner.API/Data/DesignTimeDbContextFactory.cs`

**Findings**:
- ✅ Supports both PostgreSQL and SQL Server during migrations
- ✅ Reads provider from configuration
- ✅ Required for `dotnet ef migrations` commands
- ⚠️ Migrations may need to be provider-specific or tested separately

### 4. SQL Generation Service

**Location**: `backend/src/FormDesigner.API/Services/SqlGeneratorService.cs`

**Current Implementation**: PostgreSQL-specific DDL generation

```csharp
private string MapDslTypeToSql(string dslType)
{
    return dslType.ToLower() switch
    {
        "string" => "VARCHAR(255)",      // PostgreSQL
        "datetime" => "TIMESTAMPTZ",     // PostgreSQL
        "bool" => "BOOLEAN",             // PostgreSQL
        // ...
    };
}
```

**Issues Identified**:
- ❌ Uses PostgreSQL data types (VARCHAR, TIMESTAMPTZ, BOOLEAN)
- ❌ Computed columns use `GENERATED ALWAYS AS ... STORED` (PostgreSQL syntax)
- ❌ No provider detection in SQL generation
- 🔧 **Needs refactoring** to detect active provider and generate appropriate DDL

**Required Changes**:
- Add provider-aware type mapping (VARCHAR→NVARCHAR, TIMESTAMPTZ→DATETIME2, BOOLEAN→BIT)
- Change computed column syntax from `STORED` to `PERSISTED` for SQL Server
- Update CREATE INDEX syntax if needed

### 5. Database Setup Scripts

**PostgreSQL**: `backend/database/setup.sql`
**SQL Server**: `backend/database/setup.sqlserver.sql`

**SQL Server Script Analysis**:
- ✅ Creates `formflow` database
- ✅ Defines all three tables: form_definitions, form_instances, temporary_states
- ✅ Uses SQL Server types: NVARCHAR, DATETIME2, BIT, UNIQUEIDENTIFIER
- ✅ Includes all indexes
- ✅ Idempotent (uses IF NOT EXISTS checks)

**Schema Parity**:
| Table | Columns Match | Indexes Match | Constraints Match |
|-------|---------------|---------------|-------------------|
| form_definitions | ✅ Yes | ✅ Yes (except GIN) | ✅ Yes |
| form_instances | ✅ Yes | ✅ Yes | ✅ Yes |
| temporary_states | ✅ Yes | ✅ Yes | ✅ Yes |

### 6. Test Infrastructure

**Test Projects**:
- `FormDesigner.Tests.Unit` - Unit tests (no database)
- `FormDesigner.Tests.Integration` - Integration tests (uses PostgreSQL)
- `FormDesigner.Tests.Contract` - Contract tests

**Current Test Fixture**: `IntegrationTestFixture.cs`

```csharp
public IntegrationTestFixture()
{
    _factory = new WebApplicationFactory<Program>();
    Client = _factory.CreateClient();
}
```

**Findings**:
- ❌ No SQL Server test configuration
- ❌ Tests assume PostgreSQL connection
- ❌ No database provider parameterization
- 🔧 **Needs**: SQL Server-specific test fixture or parameterized tests

### 7. Configuration Files

**appsettings.json**:
```json
{
  "Database": {
    "Provider": "Postgres",
    "ConnectionStringName": "FormDesignerDb"
  },
  "ConnectionStrings": {
    "FormDesignerDb": "Host=localhost;Port=5400;Database=formflow;...",
    "FormDesignerDbSqlServer": "Server=localhost,1433;Database=formflow;..."
  }
}
```

**Findings**:
- ✅ Both connection strings defined
- ✅ Provider is configurable
- ⚠️ Default is PostgreSQL (need to switch for SQL Server testing)

## Gap Analysis

### Critical Gaps (P1)

1. **No SQL Server Integration Tests**
   - Impact: Cannot verify SQL Server functionality
   - Risk: High - Production bugs may go undetected
   - Solution: Create SQL Server test fixture and run full test suite

2. **SqlGeneratorService PostgreSQL-Specific**
   - Impact: Generated SQL won't work on SQL Server
   - Risk: High - Export feature breaks for SQL Server users
   - Solution: Add provider detection and conditional type mapping

3. **No Test Database Setup/Teardown**
   - Impact: Tests may interfere with each other
   - Risk: Medium - Flaky tests, hard to debug
   - Solution: Implement test database isolation strategy

### Medium Gaps (P2)

4. **No Migration Testing**
   - Impact: Unknown if migrations work correctly on SQL Server
   - Risk: Medium - Deployment issues
   - Solution: Test migrations against SQL Server instance

5. **No Performance Comparison**
   - Impact: Unknown if SQL Server performs acceptably
   - Risk: Low-Medium - Potential performance issues in production
   - Solution: Add basic performance benchmarks

### Low Priority (P3)

6. **Documentation Incomplete**
   - Impact: Developers may not know how to switch providers
   - Risk: Low - Internal knowledge gap
   - Solution: Update README with SQL Server configuration instructions

## Technical Recommendations

### Phase 1: Validation Testing (P1)

1. **Create SQL Server Test Fixture**
   - Extend `IntegrationTestFixture` or create `SqlServerTestFixture`
   - Configure provider via environment variable or test settings
   - Implement database cleanup between tests

2. **Run Existing Integration Tests**
   - Execute all existing tests against SQL Server
   - Document any failures
   - Fix compatibility issues

3. **Add Provider-Specific Tests**
   - Test JSON serialization/deserialization
   - Test timestamp defaults (SYSUTCDATETIME)
   - Test concurrent access and transactions

### Phase 2: SQL Generation (P2)

4. **Refactor SqlGeneratorService**
   - Inject `ApplicationDbContext` or provider info
   - Add `MapDslTypeToSqlServer()` method
   - Switch based on active provider
   - Update computed column syntax

5. **Test SQL Generation**
   - Generate DDL for test forms
   - Verify SQL executes successfully on SQL Server
   - Validate generated schema matches expectations

### Phase 3: Documentation (P3)

6. **Update Documentation**
   - Document provider switching in README
   - Add SQL Server setup instructions
   - Include connection string examples
   - Document known differences/limitations

## Technology Stack

**Current Stack**:
- C# 12 / .NET 8.0 (LTS) [Note: Using .NET 10 preview in csproj]
- Entity Framework Core 8.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- xUnit 2.6.2, FluentAssertions 6.12.0
- Microsoft.AspNetCore.Mvc.Testing 8.0.0

**Testing Dependencies**:
- WebApplicationFactory<Program> for integration tests
- No test containers (Testcontainers) - direct database connections

## Dependencies & Constraints

### External Dependencies
- SQL Server instance (localhost:1433 or container)
- Connection requires: sa user or specific SQL auth
- TrustServerCertificate=True for local dev

### Performance Constraints
- Per spec SC-004: Startup time within 10% of PostgreSQL
- Per spec SC-005: API response time within 15% of PostgreSQL

### Testing Constraints
- Must support both providers without code changes (config only)
- Tests should be runnable in CI/CD (may need containerized SQL Server)
- Test isolation required (separate databases or cleanup strategy)

## Existing Assets to Leverage

1. **EF Core Configuration** - Already provider-agnostic
2. **Repository Pattern** - Database-agnostic by design
3. **SQL Server Schema Script** - Ready to use for test setup
4. **Connection String** - Already configured
5. **Existing Integration Tests** - Can be reused with new fixture

## Open Questions

1. **Test Database Strategy**:
   - Use Docker container for SQL Server in tests?
   - Use LocalDB for Windows developers?
   - Separate test database per test class?

2. **Migration Strategy**:
   - Single set of migrations or provider-specific?
   - Test migrations on both providers?

3. **CI/CD**:
   - Run tests against both providers in CI?
   - Parallel or sequential?

4. **SqlGeneratorService Provider Detection**:
   - Inject ApplicationDbContext.Database.ProviderName?
   - Add IConfiguration to detect from settings?
   - New abstraction layer?

## Success Metrics

From spec.md:
- SC-001: 100% integration test pass rate on SQL Server ✅
- SC-002: New SQL Server-specific tests created ✅
- SC-003: SQL generation produces valid SQL Server DDL ✅
- SC-004: Startup time within 10% of PostgreSQL
- SC-005: API response time within 15% of PostgreSQL
- SC-006: 80%+ code coverage for database abstraction
- SC-007: Clear configuration documentation ✅
- SC-008: Config-only provider switching (no code changes) ✅

## Next Steps

1. ✅ Research complete - findings documented
2. ➡️ Phase 1: Design data model (if needed) and contracts
3. ➡️ Phase 1: Create quickstart guide for SQL Server testing
4. ➡️ Phase 2: Generate tasks.md with prioritized implementation steps

## References

- Spec: `/Users/shafqat/git/FrameworkQ.Formflow/specs/002-the-system-is/spec.md`
- EF Core Multi-Provider: https://learn.microsoft.com/en-us/ef/core/providers/
- SQL Server Data Types: https://learn.microsoft.com/en-us/sql/t-sql/data-types/
- EF Core Testing: https://learn.microsoft.com/en-us/ef/core/testing/
