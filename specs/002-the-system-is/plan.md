# Implementation Plan: SQL Server Database Support with Testing

**Branch**: `002-the-system-is` | **Date**: 2025-10-07 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/Users/shafqat/git/FrameworkQ.Formflow/specs/002-the-system-is/spec.md`

## Summary

This feature validates and tests the existing SQL Server infrastructure that has been partially implemented in the FormDesigner application. The primary goal is to ensure database independence by creating comprehensive integration tests, fixing any compatibility issues, and updating the SQL generation service to produce provider-specific DDL.

**Current State**: Infrastructure code exists (EF Core configuration, SQL Server support in Program.cs, database setup script) but lacks automated testing and validation.

**Target State**: Fully tested SQL Server support with 100% integration test pass rate, provider-aware SQL generation, and documented configuration process.

## Technical Context

**Language/Version**: C# 12 / .NET 10.0 (preview) - using .NET 8.0 EF Core packages
**Primary Dependencies**:
- Entity Framework Core 8.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- xUnit 2.6.2, FluentAssertions 6.12.0, Moq 4.20.70
- Microsoft.AspNetCore.Mvc.Testing 8.0.0

**Storage**:
- Primary: PostgreSQL 15+ (existing)
- Secondary: Microsoft SQL Server 2019+ (validating)
- JSON storage: JSONB (PostgreSQL) vs NVARCHAR(MAX) (SQL Server)

**Testing**: xUnit with integration testing via WebApplicationFactory
**Target Platform**: Cross-platform (Linux/Windows server), Docker-compatible
**Project Type**: Web (backend API + frontend SPA)
**Performance Goals**:
- SQL Server startup time within 10% of PostgreSQL (SC-004)
- API response time within 15% of PostgreSQL (SC-005)

**Constraints**:
- No code changes required for provider switching (config-only)
- Maintain backward compatibility with existing PostgreSQL deployments
- 80%+ code coverage for database abstraction layer (SC-006)

**Scale/Scope**:
- 3 entity types (FormDefinition, FormInstance, TemporaryState)
- Support for both database providers
- 15 implementation tasks across 4 phases
- Estimated 8 days implementation time

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED (No constitution file exists - using default best practices)

The project constitution template exists at `.specify/memory/constitution.md` but has not been customized. This feature follows standard best practices:

- ✅ Test-first approach: Creating comprehensive tests before fixing issues
- ✅ Integration testing: Full test coverage for both database providers
- ✅ Backward compatibility: No breaking changes to existing functionality
- ✅ Documentation: Clear configuration and setup instructions
- ✅ Observability: Tests provide clear error messages and validation

**No violations identified** - this is primarily a testing and validation feature.

## Project Structure

### Documentation (this feature)

```
specs/002-the-system-is/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research output
├── data-model.md        # Phase 1 data model analysis
├── quickstart.md        # Phase 1 setup guide
├── contracts/           # Phase 1 test contracts
│   └── test-contracts.md
├── tasks.md             # Phase 2 implementation tasks
└── test-data/           # Sample test data (to be created)
    ├── simple-form.json
    ├── complex-form.json
    ├── table-form.json
    └── large-form.json
```

### Source Code (repository root)

```
backend/
├── src/
│   └── FormDesigner.API/
│       ├── Data/
│       │   ├── ApplicationDbContext.cs           [MODIFY: Already provider-aware]
│       │   ├── DesignTimeDbContextFactory.cs     [MODIFY: Already supports both]
│       │   ├── Migrations/                       [VALIDATE: Test on SQL Server]
│       │   └── Repositories/                     [NO CHANGE: Provider-agnostic]
│       ├── Services/
│       │   ├── SqlGeneratorService.cs            [MODIFY: Add provider detection]
│       │   └── ISqlGeneratorService.cs           [REVIEW: May need updates]
│       ├── Program.cs                            [NO CHANGE: Already supports both]
│       └── appsettings.json                      [NO CHANGE: Both providers configured]
└── tests/
    ├── FormDesigner.Tests.Integration/
    │   ├── SqlServerTestFixture.cs               [CREATE: New test fixture]
    │   ├── DatabaseProviderTests.cs              [CREATE: Provider detection]
    │   ├── SchemaValidationTests.cs              [CREATE: Schema validation]
    │   ├── JsonSerializationTests.cs             [CREATE: JSON handling]
    │   ├── TimestampTests.cs                     [CREATE: Timestamp defaults]
    │   ├── SqlServerGenerationTests.cs           [CREATE: SQL generation]
    │   ├── PerformanceTests.cs                   [CREATE: Performance benchmarks]
    │   ├── ErrorHandlingTests.cs                 [CREATE: Error scenarios]
    │   └── [existing tests]                      [RUN: Against SQL Server]
    ├── FormDesigner.Tests.Contract/              [RUN: Against SQL Server]
    └── FormDesigner.Tests.Unit/                  [NO CHANGE: No database]

frontend/                                          [NO CHANGE: Not affected]

database/
├── setup.sql                                      [NO CHANGE: PostgreSQL script]
└── setup.sqlserver.sql                            [VALIDATE: Already exists]
```

**Structure Decision**: Web application structure (backend/frontend/tests) is established. This feature focuses on backend database layer testing with no frontend changes required.

## Complexity Tracking

*No constitutional violations - this section intentionally left blank*

## Phase 0: Research ✅ COMPLETE

**Artifact**: [research.md](./research.md)

**Key Findings**:
1. ✅ SQL Server infrastructure partially implemented (Program.cs, ApplicationDbContext, DesignTimeDbContextFactory)
2. ✅ NuGet packages already installed (Microsoft.EntityFrameworkCore.SqlServer 8.0.0)
3. ✅ Database setup script exists and matches PostgreSQL schema
4. ❌ No SQL Server integration tests exist
5. ❌ SqlGeneratorService is PostgreSQL-specific (uses VARCHAR, TIMESTAMPTZ, BOOLEAN, STORED)
6. ⚠️ Migrations not tested on SQL Server

**Risk Assessment**: Medium - Core infrastructure exists, but lacks validation. Potential bugs in JSON serialization, timestamp handling, or query differences.

## Phase 1: Design ✅ COMPLETE

**Artifacts**:
- [data-model.md](./data-model.md) - Schema analysis and database-specific mappings
- [quickstart.md](./quickstart.md) - Setup and testing guide
- [contracts/test-contracts.md](./contracts/test-contracts.md) - Test contracts and acceptance criteria

**Key Decisions**:

1. **No New Entities**: Feature validates existing schema (FormDefinitionEntity, FormInstanceEntity, TemporaryStateEntity)

2. **Schema Parity Confirmed**:
   - Tables: ✅ Identical structure
   - Columns: ✅ Type mappings defined (JSONB→NVARCHAR(MAX), TIMESTAMPTZ→DATETIME2, BOOLEAN→BIT)
   - Indexes: ✅ Present (except GIN on SQL Server - expected)
   - Constraints: ✅ Foreign keys, defaults, NOT NULL

3. **Test Strategy**:
   - Create SqlServerTestFixture for isolated testing
   - Run existing tests against SQL Server
   - Add provider-specific validation tests
   - Measure performance and compare

4. **SQL Generation Strategy**:
   - Detect provider via ApplicationDbContext.Database.ProviderName
   - Add conditional type mapping based on provider
   - Update computed column syntax (STORED→PERSISTED for SQL Server)

## Phase 2: Tasks ✅ COMPLETE

**Artifact**: [tasks.md](./tasks.md)

**Task Summary**: 15 tasks across 4 phases
- **Phase 1 - Test Infrastructure** (P1): 6 tasks - T001 through T006
- **Phase 2 - SQL Generation** (P2): 4 tasks - T007 through T010
- **Phase 3 - Performance & Validation** (P1): 3 tasks - T011 through T013
- **Phase 4 - Documentation** (P3): 2 tasks - T014, T015

**Critical Path**:
```
T001 (SQL Server Fixture) → T004 (Run Existing Tests) → T007 (Refactor SQL Gen) → T011 (Performance) → T014 (Docs)
```

**Estimated Timeline**: 8 days (1 developer)
- Sprint 1: Days 1-3 (Core Testing)
- Sprint 2: Days 4-5 (Functional Testing)
- Sprint 3: Days 6-7 (SQL Generation)
- Sprint 4: Day 8 (Finalization)

## Implementation Approach

### Test-Driven Development Flow

1. **T001-T003**: Create test infrastructure and schema validation tests (RED - tests fail initially)
2. **T004**: Run existing tests against SQL Server (identify compatibility issues)
3. **T005-T006**: Add JSON and timestamp tests (validate database-specific behavior)
4. **T007-T010**: Fix SQL generation service (GREEN - make tests pass)
5. **T011-T013**: Validate performance and migrations (REFACTOR - optimize if needed)
6. **T014-T015**: Document and finalize (DONE - feature complete)

### Provider Detection Pattern

```csharp
// In SqlGeneratorService constructor
public SqlGeneratorService(IFormRepository formRepository, ApplicationDbContext dbContext, ILogger logger)
{
    _formRepository = formRepository;
    _dbContext = dbContext;
    _logger = logger;

    // Detect provider
    var providerName = _dbContext.Database.ProviderName ?? string.Empty;
    _isPostgreSQL = providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    _isSqlServer = providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
}

// Type mapping
private string MapDslTypeToSql(string dslType)
{
    if (_isSqlServer)
    {
        return dslType.ToLower() switch
        {
            "string" => "NVARCHAR(255)",
            "text" => "NVARCHAR(MAX)",
            "datetime" => "DATETIME2",
            "bool" => "BIT",
            // ...
        };
    }

    // PostgreSQL (existing logic)
    return dslType.ToLower() switch { /* ... */ };
}
```

### Test Isolation Strategy

Each test run creates a unique database:
```csharp
// SqlServerTestFixture
private readonly string _databaseName = $"formflow_test_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
```

Benefits:
- ✅ No test interference
- ✅ Parallel test execution possible
- ✅ Easy cleanup (drop database)

### Configuration Override Pattern

Tests use environment variables to override provider:
```csharp
Environment.SetEnvironmentVariable("Database__Provider", "SqlServer");
Environment.SetEnvironmentVariable("ConnectionStrings__FormDesignerDb", sqlServerConnectionString);
```

## Acceptance Criteria (From Spec)

**Feature Complete When**:

- [x] **AC-001**: Feature specification complete (spec.md)
- [x] **AC-002**: Research complete with findings documented (research.md)
- [x] **AC-003**: Data model analyzed and documented (data-model.md)
- [x] **AC-004**: Quickstart guide created (quickstart.md)
- [x] **AC-005**: Test contracts defined (contracts/test-contracts.md)
- [x] **AC-006**: Implementation tasks identified (tasks.md)
- [ ] **AC-007**: All P1 tasks complete and tests passing
- [ ] **AC-008**: SQL generation produces valid SQL Server DDL
- [ ] **AC-009**: Performance within thresholds (SC-004, SC-005)
- [ ] **AC-010**: Documentation complete and reviewed
- [ ] **AC-011**: Code review passed
- [ ] **AC-012**: Feature merged to main branch

## Success Metrics

From [spec.md](./spec.md):

- **SC-001**: All existing integration tests pass when configured with SQL Server (100% pass rate) ⏳
- **SC-002**: New SQL Server-specific tests validate database provider configuration, connection, and migrations ⏳
- **SC-003**: SQL generation service produces valid SQL Server DDL that can be executed without errors ⏳
- **SC-004**: Application startup time with SQL Server is comparable to PostgreSQL (within 10%) ⏳
- **SC-005**: API response times with SQL Server are comparable to PostgreSQL (within 15%) ⏳
- **SC-006**: Code coverage for database abstraction layer is at least 80% ⏳
- **SC-007**: Documentation clearly explains how to configure each database provider with working examples ⏳
- **SC-008**: Developers can switch database providers by changing configuration without code changes ⏳

Legend: ✅ Complete | ⏳ In Progress | ❌ Not Started

## Dependencies & Prerequisites

### External Dependencies
- SQL Server 2019+ instance (local or Docker)
- Connection requires SQL authentication or integrated security
- Port 1433 accessible
- TrustServerCertificate=True for local development

### Internal Dependencies
- No blocking dependencies from other features
- Can proceed independently

### Development Environment Setup
```bash
# Option 1: Docker (recommended for testing)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver-dev \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Option 2: Local SQL Server
# Install SQL Server Express or Developer Edition
# Ensure SQL Server Authentication enabled

# Setup database
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -i backend/database/setup.sqlserver.sql
```

## Risk Analysis

| Risk | Probability | Impact | Mitigation | Status |
|------|------------|--------|------------|--------|
| Existing tests fail on SQL Server | High | High | T004 identifies issues early, allows fixes | Mitigated |
| Performance below threshold | Medium | Medium | T011 measures, provides data for optimization | Monitored |
| SQL generation breaks existing code | Low | High | T010 ensures backward compatibility | Mitigated |
| JSON serialization issues | Medium | High | T005 comprehensive JSON tests | Mitigated |
| Migration incompatibilities | Low | Medium | T013 validates migrations separately | Mitigated |
| CI/CD SQL Server setup complex | Medium | Low | Docker-based CI acceptable | Mitigated |

## Next Steps

1. ✅ Planning phase complete - all artifacts generated
2. ➡️ **Next**: Begin implementation with T001 (Create SQL Server Test Fixture)
3. ➡️ Follow task order in [tasks.md](./tasks.md)
4. ➡️ Use [quickstart.md](./quickstart.md) for environment setup
5. ➡️ Reference [contracts/test-contracts.md](./contracts/test-contracts.md) for test expectations

## Progress Tracking

**Planning Phase**: ✅ Complete (2025-10-07)
- [x] Phase 0: Research
- [x] Phase 1: Design (data-model, contracts, quickstart)
- [x] Phase 2: Tasks generation
- [x] Phase 3: Plan documentation

**Implementation Phase**: ⏳ Ready to Start
- [ ] Sprint 1: Core Testing (T001-T006)
- [ ] Sprint 2: Functional Testing (T007-T010)
- [ ] Sprint 3: SQL Generation (T011-T013)
- [ ] Sprint 4: Finalization (T014-T015)

## References

**Feature Documentation**:
- [spec.md](./spec.md) - Feature specification
- [research.md](./research.md) - Research findings
- [data-model.md](./data-model.md) - Data model analysis
- [quickstart.md](./quickstart.md) - Setup guide
- [contracts/test-contracts.md](./contracts/test-contracts.md) - Test contracts
- [tasks.md](./tasks.md) - Implementation tasks

**Codebase References**:
- Program.cs:118-142 - Database provider configuration
- ApplicationDbContext.cs:33-241 - Provider-specific model configuration
- SqlGeneratorService.cs - SQL generation (needs updating)
- setup.sqlserver.sql - SQL Server schema script

**External References**:
- [EF Core Providers](https://learn.microsoft.com/en-us/ef/core/providers/)
- [SQL Server Data Types](https://learn.microsoft.com/en-us/sql/t-sql/data-types/)
- [EF Core Testing](https://learn.microsoft.com/en-us/ef/core/testing/)

---

**Plan Version**: 1.0
**Last Updated**: 2025-10-07
**Status**: Ready for Implementation
