# Feature Specification: SQL Server Database Support with Testing

**Feature Branch**: `002-the-system-is`
**Created**: 2025-10-07
**Status**: Draft
**Input**: User description: "The system is desgined to work with postgres sql. We need to make it database independednt. Lets implement sql server based database store and run tests to verify. Partial implementation has been done, you need to write test cases and verify sql server impementation"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify SQL Server Integration (Priority: P1)

As a developer, I need to verify that the existing SQL Server integration works correctly so that I can be confident that the application supports both PostgreSQL and SQL Server databases.

**Why this priority**: This is the foundation of database independence. The infrastructure code exists but hasn't been validated through automated tests.

**Independent Test**: Can be fully tested by running the application against SQL Server, performing CRUD operations on forms, and verifying all functionality works identically to PostgreSQL.

**Acceptance Scenarios**:

1. **Given** SQL Server is configured as the database provider, **When** the application starts, **Then** the DbContext connects successfully and all tables are created
2. **Given** SQL Server is active, **When** I create a form definition, **Then** the form is persisted with correct JSON serialization
3. **Given** SQL Server contains form data, **When** I query forms via the API, **Then** all forms are retrieved correctly

---

### User Story 2 - Test Database-Specific Configuration (Priority: P1)

As a developer, I need comprehensive tests that verify database-specific behaviors (JSON handling, timestamps, data types) so that I can ensure both databases work correctly with our entity framework configuration.

**Why this priority**: Different databases handle JSON, timestamps, and data types differently. Without tests, silent data corruption or functionality issues could occur.

**Independent Test**: Can be tested independently by creating test fixtures for each database provider and verifying schema migrations, column types, and default values match expectations.

**Acceptance Scenarios**:

1. **Given** EF Core migrations for SQL Server, **When** migrations are applied, **Then** column types match SQL Server conventions (NVARCHAR(MAX), DATETIME2, BIT)
2. **Given** a form definition with JSON data in SQL Server, **When** retrieving the form, **Then** JSON is correctly serialized and deserialized
3. **Given** timestamp columns in SQL Server, **When** inserting records, **Then** SYSUTCDATETIME() defaults are applied correctly

---

### User Story 3 - SQL Generation for SQL Server (Priority: P2)

As a developer, I need the SQL generation service to produce SQL Server-compatible DDL statements so that table and grid widgets can generate database schemas for either database platform.

**Why this priority**: The current SqlGeneratorService is PostgreSQL-specific. While important, this can be addressed after core CRUD operations are validated.

**Independent Test**: Can be tested by requesting SQL DDL for forms with table/grid widgets and verifying the generated SQL uses SQL Server syntax (e.g., NVARCHAR instead of VARCHAR, DATETIME2 instead of TIMESTAMPTZ).

**Acceptance Scenarios**:

1. **Given** a form with table widgets on SQL Server, **When** generating SQL DDL, **Then** data types are mapped to SQL Server types (VARCHAR→NVARCHAR, TIMESTAMPTZ→DATETIME2)
2. **Given** a table with computed columns on SQL Server, **When** generating SQL, **Then** computed columns use SQL Server syntax (PERSISTED instead of STORED)
3. **Given** a form with indexes on SQL Server, **When** generating SQL DDL, **Then** indexes use SQL Server CREATE INDEX syntax

---

### User Story 4 - Integration Test Suite for SQL Server (Priority: P1)

As a developer, I need a comprehensive integration test suite that runs against SQL Server so that I can verify all API endpoints and business logic work correctly with SQL Server.

**Why this priority**: Integration tests are critical for confidence that the application works end-to-end with SQL Server before deployment.

**Independent Test**: Can be tested by creating a SQL Server-specific test fixture, running the full integration test suite, and verifying all tests pass.

**Acceptance Scenarios**:

1. **Given** integration tests configured for SQL Server, **When** running the test suite, **Then** all existing integration tests pass
2. **Given** SQL Server test database, **When** tests execute, **Then** test isolation works correctly (setup/teardown)
3. **Given** concurrent test execution, **When** multiple tests run, **Then** no database conflicts occur

---

### User Story 5 - Configuration Documentation (Priority: P3)

As a developer or DevOps engineer, I need clear documentation on how to configure and switch between PostgreSQL and SQL Server so that I can deploy the application to different environments.

**Why this priority**: Documentation is important but can be added after technical implementation is complete and validated.

**Independent Test**: Can be tested by following the documentation to switch database providers and verifying the application works correctly.

**Acceptance Scenarios**:

1. **Given** configuration documentation, **When** changing Database:Provider to "SqlServer", **Then** application connects to SQL Server successfully
2. **Given** connection string examples, **When** configuring appsettings.json, **Then** both database providers can be configured correctly
3. **Given** migration instructions, **When** switching databases, **Then** developers understand how to apply migrations

---

### Edge Cases

- What happens when SQL Server is unavailable during application startup?
- How does the system handle SQL Server-specific query syntax differences (e.g., OFFSET/FETCH vs LIMIT)?
- What happens when JSON data exceeds NVARCHAR(MAX) limits in SQL Server?
- How does the system handle transaction isolation differences between PostgreSQL and SQL Server?
- What happens when concurrent modifications occur to the same form in SQL Server?
- How does the SQL generator handle SQL Server reserved keywords as column names?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support both PostgreSQL and SQL Server as database providers
- **FR-002**: System MUST allow database provider selection via configuration (appsettings.json)
- **FR-003**: System MUST correctly map Entity Framework entities to both PostgreSQL and SQL Server schemas
- **FR-004**: System MUST serialize and deserialize JSON data correctly in both database types (JSONB vs NVARCHAR(MAX))
- **FR-005**: System MUST apply appropriate default values for timestamps in both databases (NOW() vs SYSUTCDATETIME())
- **FR-006**: System MUST generate database-specific SQL DDL when using the SQL export feature
- **FR-007**: System MUST handle computed columns correctly for both database types
- **FR-008**: System MUST create appropriate indexes for both database types
- **FR-009**: Integration tests MUST run successfully against both PostgreSQL and SQL Server
- **FR-010**: System MUST handle database connection failures gracefully with meaningful error messages
- **FR-011**: System MUST support database-specific retry policies (especially for SQL Server transient failures)
- **FR-012**: System MUST validate that required NuGet packages are installed for each database provider

### Key Entities *(include if feature involves data)*

- **DatabaseProvider**: Enumeration/configuration representing the active database type (Postgres, SqlServer)
- **ApplicationDbContext**: EF Core context that adapts to the configured database provider
- **SqlGeneratorService**: Service that generates database-specific DDL statements
- **TestFixture**: Test infrastructure supporting multiple database providers

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing integration tests pass when configured with SQL Server (100% pass rate)
- **SC-002**: New SQL Server-specific tests validate database provider configuration, connection, and migrations
- **SC-003**: SQL generation service produces valid SQL Server DDL that can be executed without errors
- **SC-004**: Application startup time with SQL Server is comparable to PostgreSQL (within 10%)
- **SC-005**: API response times with SQL Server are comparable to PostgreSQL (within 15%)
- **SC-006**: Code coverage for database abstraction layer is at least 80%
- **SC-007**: Documentation clearly explains how to configure each database provider with working examples
- **SC-008**: Developers can switch database providers by changing configuration without code changes
