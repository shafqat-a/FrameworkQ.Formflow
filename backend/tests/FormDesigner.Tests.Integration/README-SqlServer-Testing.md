# SQL Server Integration Testing

## Overview

This document explains how to run the existing integration tests against SQL Server to validate database independence.

## Test Organization

### Tests Using SQL Server (SqlServerTestFixture)
These tests are specifically designed for SQL Server validation:
- `DatabaseProviderTests.cs` - Provider detection and configuration
- `SchemaValidationTests.cs` - Schema structure and constraints
- `SqlServerTestFixtureTests.cs` - Test fixture validation

### Tests Using PostgreSQL (IntegrationTestFixture)
These tests currently run against PostgreSQL:
- `CreateFormWithFieldTests.cs`
- `DeleteOperationsTests.cs`
- `EnhancedFeaturesTests.cs`
- `FormValidationTests.cs`
- `MultiPageFormWithTableTests.cs`
- `SqlGenerationTests.cs`
- `YamlExportValidationTests.cs`

## Running Tests Against SQL Server

### Prerequisites

1. **SQL Server Instance**: Local or Docker
   ```bash
   # Docker option (recommended)
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
     -p 1433:1433 --name sqlserver-test \
     -d mcr.microsoft.com/mssql/server:2019-latest
   ```

2. **Environment Variables**:
   ```bash
   export SQLSERVER_HOST="localhost,1433"
   export SQLSERVER_USER="sa"
   export SQLSERVER_PASSWORD="YourStrong!Passw0rd"
   ```

### Running SQL Server-Specific Tests

```bash
cd backend/tests/FormDesigner.Tests.Integration

# Run all SQL Server tests
dotnet test --filter "FullyQualifiedName~SqlServer"

# Run specific test categories
dotnet test --filter "FullyQualifiedName~DatabaseProviderTests"
dotnet test --filter "FullyQualifiedName~SchemaValidationTests"
```

### Running Existing Tests with SQL Server

To run the existing integration tests against SQL Server, you have two options:

#### Option 1: Create SQL Server Variants (Recommended for CI/CD)

Create duplicate test classes that use `SqlServerTestFixture` instead of `IntegrationTestFixture`:

```csharp
// Example: CreateFormWithFieldTests_SqlServer.cs
public class CreateFormWithFieldTests_SqlServer : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;

    public CreateFormWithFieldTests_SqlServer(SqlServerTestFixture fixture)
    {
        _client = fixture.Client;
    }

    // Copy test methods from CreateFormWithFieldTests.cs
}
```

#### Option 2: Parameterized Test Fixture (Advanced)

Create a theory-based approach that runs tests against both providers:

```csharp
public class DatabaseProviderTheoryAttribute : TheoryAttribute
{
    public DatabaseProviderTheoryAttribute()
    {
        // Skip if no SQL Server available
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SQLSERVER_HOST")))
        {
            Skip = "SQL Server not configured";
        }
    }
}
```

## Test Execution Matrix

| Test File | PostgreSQL | SQL Server | Status |
|-----------|-----------|------------|--------|
| CreateFormWithFieldTests | ✅ | ⏳ | Needs SQL Server variant |
| DeleteOperationsTests | ✅ | ⏳ | Needs SQL Server variant |
| EnhancedFeaturesTests | ✅ | ⏳ | Needs SQL Server variant |
| FormValidationTests | ✅ | ⏳ | Needs SQL Server variant |
| MultiPageFormWithTableTests | ✅ | ⏳ | Needs SQL Server variant |
| SqlGenerationTests | ✅ | ⏳ | Needs SQL Server variant |
| YamlExportValidationTests | ✅ | ⏳ | Needs SQL Server variant |
| DatabaseProviderTests | N/A | ✅ | Complete |
| SchemaValidationTests | N/A | ✅ | Complete |

## Expected Behavior

All existing integration tests should pass on SQL Server without modification because:

1. **EF Core Abstracts Provider Differences**: Entity Framework Core handles provider-specific SQL generation
2. **Provider-Agnostic Repositories**: Repository layer doesn't contain provider-specific code
3. **Configuration-Based Switching**: Database provider is selected via configuration only

## Known Differences (Expected)

The following differences are expected and acceptable:

1. **Index Types**: GIN indexes (PostgreSQL-specific) won't exist on SQL Server
2. **JSON Querying**: PostgreSQL JSONB operators won't work on SQL Server's NVARCHAR(MAX)
3. **Performance**: SQL Server may have slightly different performance characteristics
4. **Error Messages**: Provider-specific error messages may differ

## Troubleshooting

### Connection Issues
```bash
# Test SQL Server connectivity
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION"
```

### Database Not Created
The `SqlServerTestFixture` automatically creates test databases. If tests fail with "database not found":
1. Check SQL Server is running
2. Verify connection string environment variables
3. Check SQL Server authentication mode (must allow SQL authentication)

### Migration Issues
If migrations fail on SQL Server:
```bash
cd backend/src/FormDesigner.API
export Database__Provider=SqlServer
export ConnectionStrings__FormDesignerDb="Server=localhost,1433;Database=formflow_test;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
dotnet ef database update
```

## CI/CD Integration

### GitHub Actions Example

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
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run SQL Server Tests
        run: dotnet test backend/tests/FormDesigner.Tests.Integration
        env:
          SQLSERVER_HOST: localhost,1433
          SQLSERVER_USER: sa
          SQLSERVER_PASSWORD: YourStrong!Passw0rd
```

## Next Steps

1. ✅ Create SQL Server test fixture (T001)
2. ✅ Create provider detection tests (T002)
3. ✅ Create schema validation tests (T003)
4. ⏳ Run existing tests against SQL Server (T004) - **This Task**
   - Manual verification needed with SQL Server instance
   - Create SQL Server variants of existing tests for automated CI/CD
5. ⏳ Create JSON serialization tests (T005)
6. ⏳ Create timestamp tests (T006)

## Success Criteria (from spec.md)

- **SC-001**: All existing integration tests pass when configured with SQL Server (100% pass rate) ⏳
- **SC-002**: New SQL Server-specific tests validate database provider configuration ✅
- **SC-008**: Developers can switch database providers by changing configuration ✅

## Documentation References

- [Test Contracts](../../specs/002-the-system-is/contracts/test-contracts.md) - TC-601 through TC-604
- [Feature Spec](../../specs/002-the-system-is/spec.md) - User Story 4
- [Quickstart Guide](../../specs/002-the-system-is/quickstart.md) - SQL Server setup
