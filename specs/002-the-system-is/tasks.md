# Implementation Tasks: SQL Server Database Support

**Feature**: SQL Server Database Support with Testing
**Branch**: `002-the-system-is`
**Date**: 2025-10-07
**Status**: Ready for Implementation

## Task Summary

Total tasks: 15 | P1: 9 | P2: 4 | P3: 2

## Phase 1: Test Infrastructure (P1)

### T001: Create SQL Server Test Fixture
**GitHub Issue**: [#18](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/18)
**Priority**: P1
**Depends on**: None
**Estimated effort**: 2 hours

**Description**: Create a dedicated test fixture for SQL Server integration tests that manages database lifecycle and provides isolated test environment.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/SqlServerTestFixture.cs`
2. Implement `IClassFixture<SqlServerTestFixture>` pattern
3. Configure WebApplicationFactory with SQL Server provider
4. Generate unique database name per test run (e.g., `formflow_test_{timestamp}`)
5. Implement database creation in constructor
6. Implement database cleanup in Dispose()

**Acceptance Criteria**:
- ✅ Fixture creates isolated test database
- ✅ Each test run uses unique database name
- ✅ Database cleaned up after tests complete
- ✅ HttpClient provided for API testing
- ✅ Connection string configurable via environment variables

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/SqlServerTestFixture.cs`

**Reference**: `contracts/test-contracts.md` - SqlServerTestFixture contract

---

### T002: Create Database Provider Tests
**GitHub Issue**: [#19](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/19)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 1 hour

**Description**: Implement tests that verify database provider configuration and detection.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/DatabaseProviderTests.cs`
2. Implement TC-001: Provider detection for PostgreSQL
3. Implement TC-002: Provider detection for SQL Server
4. Implement TC-003: Case-insensitive provider names
5. Implement TC-004: Invalid provider error handling

**Acceptance Criteria**:
- ✅ Tests verify ApplicationDbContext.Database.ProviderName
- ✅ Tests pass for both PostgreSQL and SQL Server
- ✅ Provider name variations handled correctly
- ✅ Invalid provider throws clear exception

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/DatabaseProviderTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-001 through TC-004

---

### T003: Create Schema Validation Tests
**GitHub Issue**: [#20](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/20)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 3 hours

**Description**: Implement tests that verify database schema is correct for both providers.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/SchemaValidationTests.cs`
2. Implement TC-101: Verify tables exist
3. Implement TC-102: Verify columns and data types
4. Implement TC-103: Verify indexes exist
5. Implement TC-104: Verify foreign key constraints
6. Query INFORMATION_SCHEMA views for validation
7. Compare actual schema vs expected schema

**Acceptance Criteria**:
- ✅ All tables present in both providers
- ✅ Column types match provider-specific mappings
- ✅ Indexes present (except GIN on SQL Server)
- ✅ Foreign keys correctly configured
- ✅ NOT NULL constraints match

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/SchemaValidationTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-101 through TC-104, `data-model.md`

---

### T004: Run Existing Tests Against SQL Server
**GitHub Issue**: [#21](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/21)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 2 hours

**Description**: Configure and run all existing integration tests against SQL Server to identify compatibility issues.

**Implementation**:
1. Create test configuration for SQL Server
2. Update existing test classes to support parameterized providers (if needed)
3. Run all tests in `FormDesigner.Tests.Integration`
4. Run all tests in `FormDesigner.Tests.Contract`
5. Document any failures
6. Create bug fixes for compatibility issues

**Acceptance Criteria**:
- ✅ All existing integration tests pass on SQL Server
- ✅ All contract tests pass on SQL Server
- ✅ Test results documented
- ✅ Any issues identified and fixed

**Files to modify**:
- `backend/tests/FormDesigner.Tests.Integration/IntegrationTestFixture.cs` (may need updates)
- Various test files (as needed for fixes)

**Reference**: `contracts/test-contracts.md` - TC-601 through TC-604

---

### T005: Create JSON Serialization Tests
**GitHub Issue**: [#22](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/22)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 2 hours

**Description**: Implement comprehensive tests for JSON storage and retrieval across both providers.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/JsonSerializationTests.cs`
2. Implement TC-301: Simple JSON roundtrip
3. Implement TC-302: Complex JSON with nesting, arrays, special characters
4. Implement TC-303: Large JSON payload (~500KB)
5. Implement TC-304: Null handling
6. Create test data files in `specs/002-the-system-is/test-data/`

**Acceptance Criteria**:
- ✅ JSON roundtrip preserves structure
- ✅ Special characters handled (unicode, emoji, quotes, newlines)
- ✅ Large payloads supported
- ✅ Null values preserved correctly
- ✅ Tests pass on both providers

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/JsonSerializationTests.cs`
- `specs/002-the-system-is/test-data/complex-form.json`
- `specs/002-the-system-is/test-data/large-form.json`

**Reference**: `contracts/test-contracts.md` - TC-301 through TC-304

---

### T006: Create Timestamp Tests
**GitHub Issue**: [#23](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/23)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 1.5 hours

**Description**: Implement tests to verify timestamp default values and UTC handling.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/TimestampTests.cs`
2. Implement TC-401: PostgreSQL CreatedAt default (NOW())
3. Implement TC-402: SQL Server CreatedAt default (SYSUTCDATETIME())
4. Implement TC-403: UpdatedAt manual setting
5. Implement TC-404: Timestamp ordering
6. Query database directly to verify defaults applied

**Acceptance Criteria**:
- ✅ CreatedAt set automatically by database
- ✅ Timestamp values are UTC
- ✅ UpdatedAt updated correctly
- ✅ Ordering by timestamp works correctly
- ✅ Tests pass on both providers

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/TimestampTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-401 through TC-404

---

## Phase 2: SQL Generation Updates (P2)

### T007: Refactor SqlGeneratorService for Provider Detection
**GitHub Issue**: [#24](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/24)
**Priority**: P2
**Depends on**: T001 (#18), T004 (#21)
**Estimated effort**: 3 hours

**Description**: Update SqlGeneratorService to detect active database provider and generate appropriate SQL syntax.

**Implementation**:
1. Modify `backend/src/FormDesigner.API/Services/SqlGeneratorService.cs`
2. Inject ApplicationDbContext or detect provider via IConfiguration
3. Add provider detection logic in constructor
4. Create `MapDslTypeToSqlServer()` method
5. Update `MapDslTypeToSql()` to switch based on provider
6. Update computed column generation for SQL Server (PERSISTED vs STORED)

**Acceptance Criteria**:
- ✅ Service detects active provider
- ✅ PostgreSQL type mapping unchanged
- ✅ SQL Server types: NVARCHAR, DATETIME2, BIT, INT
- ✅ Computed columns use correct syntax per provider
- ✅ No breaking changes to existing functionality

**Files to modify**:
- `backend/src/FormDesigner.API/Services/SqlGeneratorService.cs`

**Type Mapping**:
| DSL Type | PostgreSQL | SQL Server |
|----------|-----------|------------|
| string | VARCHAR(255) | NVARCHAR(255) |
| text | TEXT | NVARCHAR(MAX) |
| integer | INTEGER | INT |
| decimal | NUMERIC(18,2) | DECIMAL(18,2) |
| datetime | TIMESTAMPTZ | DATETIME2 |
| bool | BOOLEAN | BIT |

**Reference**: `research.md` - SQL Generation Service Analysis

---

### T008: Update ISqlGeneratorService Interface
**GitHub Issue**: [#25](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/25)
**Priority**: P2
**Depends on**: T007 (#24)
**Estimated effort**: 0.5 hours

**Description**: Update interface if needed to support provider-specific SQL generation.

**Implementation**:
1. Review `backend/src/FormDesigner.API/Services/ISqlGeneratorService.cs`
2. Add methods if needed (e.g., `GenerateSqlAsync(string formId, string provider)`)
3. Update XML documentation

**Acceptance Criteria**:
- ✅ Interface supports provider-specific generation
- ✅ Backward compatibility maintained
- ✅ Documentation updated

**Files to modify**:
- `backend/src/FormDesigner.API/Services/ISqlGeneratorService.cs`

---

### T009: Create SQL Generation Tests
**GitHub Issue**: [#26](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/26)
**Priority**: P2
**Depends on**: T007 (#24), T008 (#25)
**Estimated effort**: 2.5 hours

**Description**: Implement tests to verify SQL DDL generation for both providers.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/SqlServerGenerationTests.cs`
2. Implement TC-501: PostgreSQL SQL generation
3. Implement TC-502: SQL Server SQL generation
4. Implement TC-503: Type mapping correctness
5. Implement TC-504: Computed column syntax
6. Verify generated SQL can be executed successfully

**Acceptance Criteria**:
- ✅ Generated SQL uses correct types per provider
- ✅ CREATE TABLE syntax valid
- ✅ CREATE INDEX syntax valid
- ✅ Computed columns use correct syntax
- ✅ Generated SQL executes without errors

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/SqlServerGenerationTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-501 through TC-504

---

### T010: Update Existing SQL Generation Tests
**GitHub Issue**: [#27](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/27)
**Priority**: P2
**Depends on**: T007 (#24)
**Estimated effort**: 1 hour

**Description**: Update existing SqlGenerationTests to work with provider-aware service.

**Implementation**:
1. Modify `backend/tests/FormDesigner.Tests.Integration/SqlGenerationTests.cs`
2. Update assertions to handle provider-specific output
3. Parameterize tests if needed
4. Ensure backward compatibility

**Acceptance Criteria**:
- ✅ All existing tests still pass
- ✅ Tests work for both providers
- ✅ No regressions introduced

**Files to modify**:
- `backend/tests/FormDesigner.Tests.Integration/SqlGenerationTests.cs`

---

## Phase 3: Performance & Validation (P1)

### T011: Create Performance Benchmark Tests
**GitHub Issue**: [#28](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/28)
**Priority**: P1
**Depends on**: T004 (#21)
**Estimated effort**: 2 hours

**Description**: Implement performance tests to verify SQL Server meets performance criteria.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/PerformanceTests.cs`
2. Implement TC-801: Measure PostgreSQL startup time
3. Implement TC-802: Measure SQL Server startup time
4. Implement TC-803: Measure PostgreSQL API response time
5. Implement TC-804: Measure SQL Server API response time
6. Compare and assert within thresholds (10% startup, 15% response time)

**Acceptance Criteria**:
- ✅ Startup time measured for both providers
- ✅ API response time measured for both providers
- ✅ SQL Server within 10% of PostgreSQL startup
- ✅ SQL Server within 15% of PostgreSQL response time
- ✅ Metrics logged for analysis

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/PerformanceTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-801 through TC-804, `spec.md` - SC-004, SC-005

---

### T012: Create Error Handling Tests
**GitHub Issue**: [#29](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/29)
**Priority**: P1
**Depends on**: T001 (#18)
**Estimated effort**: 1.5 hours

**Description**: Implement tests for database error scenarios.

**Implementation**:
1. Create `backend/tests/FormDesigner.Tests.Integration/ErrorHandlingTests.cs`
2. Implement TC-701: Connection failure handling
3. Implement TC-702: Database does not exist
4. Implement TC-703: Schema version mismatch
5. Verify error messages are clear and actionable

**Acceptance Criteria**:
- ✅ Connection failures handled gracefully
- ✅ Missing database detected with clear error
- ✅ Pending migrations detected
- ✅ Error messages guide user to solution
- ✅ Tests pass on both providers

**Files to create**:
- `backend/tests/FormDesigner.Tests.Integration/ErrorHandlingTests.cs`

**Reference**: `contracts/test-contracts.md` - TC-701 through TC-703

---

### T013: Validate Migrations on SQL Server
**GitHub Issue**: [#30](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/30)
**Priority**: P1
**Depends on**: None
**Estimated effort**: 1 hour

**Description**: Test EF Core migrations work correctly on SQL Server.

**Implementation**:
1. Create fresh SQL Server database
2. Run `dotnet ef database update` with SQL Server provider
3. Verify schema matches expectations
4. Compare with manual setup.sqlserver.sql
5. Document any discrepancies

**Acceptance Criteria**:
- ✅ Migrations apply successfully
- ✅ Schema matches manual script
- ✅ No migration errors
- ✅ Idempotent (can run multiple times)

**Commands**:
```bash
cd backend/src/FormDesigner.API
export Database__Provider=SqlServer
dotnet ef database update
```

**Reference**: `data-model.md` - Migration Strategy

---

## Phase 4: Documentation (P3)

### T014: Update Configuration Documentation
**GitHub Issue**: [#31](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/31)
**Priority**: P3
**Depends on**: All P1/P2 tasks
**Estimated effort**: 1.5 hours

**Description**: Document how to configure and use SQL Server provider.

**Implementation**:
1. Update `backend/database/README.md` with SQL Server instructions
2. Add examples to main README.md
3. Document connection string format
4. Document provider switching steps
5. Add troubleshooting section

**Acceptance Criteria**:
- ✅ Clear instructions for SQL Server setup
- ✅ Connection string examples provided
- ✅ Provider switching documented
- ✅ Troubleshooting guide included
- ✅ Quick start guide easy to follow

**Files to modify**:
- `backend/database/README.md`
- `README.md`

**Reference**: `quickstart.md`, `spec.md` - User Story 5

---

### T015: Create Test Data Files
**GitHub Issue**: [#32](https://github.com/shafqat-a/FrameworkQ.Formflow/issues/32)
**Priority**: P3
**Depends on**: None
**Estimated effort**: 1 hour

**Description**: Create sample JSON files for testing.

**Implementation**:
1. Create `specs/002-the-system-is/test-data/` directory
2. Create `simple-form.json` - basic form for CRUD tests
3. Create `complex-form.json` - nested structure with special characters
4. Create `table-form.json` - form with table widget for SQL generation
5. Create `large-form.json` - large payload for performance tests

**Acceptance Criteria**:
- ✅ All test data files created
- ✅ Valid JSON format
- ✅ Cover different test scenarios
- ✅ Referenced in tests

**Files to create**:
- `specs/002-the-system-is/test-data/simple-form.json`
- `specs/002-the-system-is/test-data/complex-form.json`
- `specs/002-the-system-is/test-data/table-form.json`
- `specs/002-the-system-is/test-data/large-form.json`

**Reference**: `contracts/test-contracts.md` - Test Data Location

---

## Task Dependency Graph

```
T001 (SQL Server Test Fixture)
├── T002 (Provider Tests)
├── T003 (Schema Validation Tests)
├── T004 (Run Existing Tests)
│   ├── T007 (Refactor SqlGeneratorService)
│   │   ├── T008 (Update Interface)
│   │   ├── T009 (SQL Generation Tests)
│   │   └── T010 (Update Existing Tests)
│   ├── T011 (Performance Tests)
│   └── T014 (Documentation)
├── T005 (JSON Tests)
├── T006 (Timestamp Tests)
└── T012 (Error Handling Tests)

T013 (Validate Migrations) - Independent
T015 (Test Data Files) - Independent
```

## Implementation Order

### Sprint 1 (Core Testing - Days 1-3)
1. T001: Create SQL Server Test Fixture
2. T015: Create Test Data Files
3. T002: Create Database Provider Tests
4. T003: Create Schema Validation Tests
5. T013: Validate Migrations on SQL Server

### Sprint 2 (Functional Testing - Days 4-5)
6. T004: Run Existing Tests Against SQL Server
7. T005: Create JSON Serialization Tests
8. T006: Create Timestamp Tests
9. T012: Create Error Handling Tests

### Sprint 3 (SQL Generation - Days 6-7)
10. T007: Refactor SqlGeneratorService
11. T008: Update ISqlGeneratorService Interface
12. T009: Create SQL Generation Tests
13. T010: Update Existing SQL Generation Tests

### Sprint 4 (Finalization - Day 8)
14. T011: Create Performance Benchmark Tests
15. T014: Update Configuration Documentation

## Success Criteria

All tasks complete when:
- ✅ All P1 tasks complete (critical path)
- ✅ All P2 tasks complete (SQL generation)
- ✅ All tests pass on both PostgreSQL and SQL Server
- ✅ Performance benchmarks met (SC-004, SC-005)
- ✅ Documentation complete and accurate
- ✅ Code coverage ≥80% for database abstraction (SC-006)

## Risk Mitigation

| Risk | Mitigation | Owner |
|------|-----------|-------|
| Tests fail on SQL Server | T004 identifies issues early | Developer |
| Performance below threshold | T011 measures, allows optimization | Developer |
| SQL generation breaks existing code | T010 ensures backward compatibility | Developer |
| Migration issues | T013 validates migrations separately | Developer |

## Test Execution Checklist

Before marking feature complete:
- [ ] All P1 tests passing on PostgreSQL
- [ ] All P1 tests passing on SQL Server
- [ ] All P2 tests passing on both providers
- [ ] Performance within thresholds
- [ ] Documentation reviewed
- [ ] Code review complete
- [ ] Integration tests green in CI/CD

## References

- Feature Spec: `specs/002-the-system-is/spec.md`
- Research: `specs/002-the-system-is/research.md`
- Data Model: `specs/002-the-system-is/data-model.md`
- Test Contracts: `specs/002-the-system-is/contracts/test-contracts.md`
- Quickstart: `specs/002-the-system-is/quickstart.md`
