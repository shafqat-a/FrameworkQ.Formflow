# Quickstart: SQL Server Testing

**Feature**: SQL Server Database Support with Testing
**Branch**: `002-the-system-is`
**Date**: 2025-10-07

## Overview

This guide helps you quickly set up and test SQL Server support for the FormDesigner application. Follow these steps to validate that the application works correctly with SQL Server.

## Prerequisites

### Required Software
- .NET 10.0 SDK (or .NET 8.0 LTS)
- SQL Server 2019+ (or Docker with SQL Server image)
- Git

### Optional Tools
- Azure Data Studio or SQL Server Management Studio
- Docker Desktop (if using containerized SQL Server)

## Quick Setup (5 Minutes)

### Option 1: Docker SQL Server (Recommended)

```bash
# 1. Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver-test \
  -d mcr.microsoft.com/mssql/server:2019-latest

# 2. Wait for SQL Server to start (about 30 seconds)
sleep 30

# 3. Verify SQL Server is running
docker exec -it sqlserver-test /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" \
  -Q "SELECT @@VERSION"
```

### Option 2: Local SQL Server

If you have SQL Server installed locally:

```bash
# Verify SQL Server is running
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION"
```

## Database Setup

### 1. Create Database and Schema

```bash
cd backend/database

# Execute SQL Server setup script
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -i setup.sqlserver.sql

# Verify tables were created
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "USE formflow; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;"
```

**Expected Output**:
```
TABLE_NAME
form_definitions
form_instances
temporary_states
```

### 2. Configure Application

Edit `backend/src/FormDesigner.API/appsettings.json`:

```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStringName": "FormDesignerDbSqlServer"
  },
  "ConnectionStrings": {
    "FormDesignerDbSqlServer": "Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  }
}
```

**Or use environment variable** (recommended for tests):

```bash
export Database__Provider=SqlServer
export ConnectionStrings__FormDesignerDb="Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

## Running the Application

### 1. Start the API

```bash
cd backend/src/FormDesigner.API
dotnet run
```

**Expected Output**:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (5ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT 1
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### 2. Test Basic Connectivity

```bash
# Health check
curl https://localhost:5001/api/forms

# Should return empty array if database is empty
# Expected: []
```

### 3. Create a Test Form

```bash
curl -X POST https://localhost:5001/api/forms \
  -H "Content-Type: application/json" \
  -d '{
    "form": {
      "id": "test-form-sqlserver",
      "title": "SQL Server Test Form",
      "version": "1.0",
      "pages": [
        {
          "id": "page-1",
          "title": "Test Page",
          "sections": [
            {
              "id": "section-1",
              "title": "Test Section",
              "widgets": [
                {
                  "type": "text",
                  "id": "name",
                  "label": "Name"
                }
              ]
            }
          ]
        }
      ]
    }
  }'
```

### 4. Verify Data Persisted

```bash
# Query via API
curl https://localhost:5001/api/forms/test-form-sqlserver

# Or query database directly
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "USE formflow; SELECT form_id, version, is_committed FROM form_definitions;"
```

## Running Tests

### 1. Configure Test Environment

Create `backend/tests/FormDesigner.Tests.Integration/appsettings.Test.json`:

```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStringName": "FormDesignerDbSqlServer"
  },
  "ConnectionStrings": {
    "FormDesignerDbSqlServer": "Server=localhost,1433;Database=formflow_test;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  }
}
```

**Note**: Use separate test database (`formflow_test`)

### 2. Create Test Database

```bash
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "CREATE DATABASE formflow_test;"

sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -d formflow_test \
  -i backend/database/setup.sqlserver.sql
```

### 3. Run Integration Tests

```bash
cd backend/tests/FormDesigner.Tests.Integration

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~SqlGenerationTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### 4. Verify Test Results

**Expected Output**:
```
Passed!  - Failed:     0, Passed:    XX, Skipped:     0, Total:    XX
```

## Common Scenarios

### Scenario 1: Switch Between Providers

**Switch to SQL Server**:
```bash
export Database__Provider=SqlServer
dotnet run
```

**Switch back to PostgreSQL**:
```bash
export Database__Provider=Postgres
dotnet run
```

### Scenario 2: Test SQL Generation

```bash
# Create form with table widget
curl -X POST https://localhost:5001/api/forms \
  -H "Content-Type: application/json" \
  -d @specs/002-the-system-is/test-data/table-form.json

# Export SQL DDL
curl https://localhost:5001/api/export/table-test-form/sql \
  -o generated-schema.sql

# Verify SQL syntax
cat generated-schema.sql
```

### Scenario 3: Test JSON Serialization

```bash
# Create form with complex JSON
curl -X POST https://localhost:5001/api/forms \
  -H "Content-Type: application/json" \
  -d '{
    "form": {
      "id": "json-test",
      "title": "JSON Test",
      "version": "1.0",
      "metadata": {
        "author": "Test User",
        "tags": ["test", "json", "special-chars"],
        "description": "Testing unicode: 你好, emoji: 🎉, quotes: \"quoted\""
      },
      "pages": [...]
    }
  }'

# Retrieve and verify JSON preserved
curl https://localhost:5001/api/forms/json-test | jq '.metadata'
```

### Scenario 4: Performance Comparison

```bash
# Measure PostgreSQL
export Database__Provider=Postgres
time dotnet run &
sleep 5
time curl https://localhost:5001/api/forms
killall dotnet

# Measure SQL Server
export Database__Provider=SqlServer
time dotnet run &
sleep 5
time curl https://localhost:5001/api/forms
killall dotnet
```

## Verification Checklist

Use this checklist to verify SQL Server support:

- [ ] SQL Server container/instance running
- [ ] Database and schema created successfully
- [ ] Application starts with SQL Server provider
- [ ] Can create form via API
- [ ] Can retrieve form via API
- [ ] Can update form via API
- [ ] Can delete form (soft delete) via API
- [ ] JSON data serializes/deserializes correctly
- [ ] Timestamps use SYSUTCDATETIME() defaults
- [ ] Foreign key constraints enforced
- [ ] All integration tests pass
- [ ] SQL generation produces valid SQL Server DDL
- [ ] Performance within acceptable range (±15%)

## Troubleshooting

### Issue: Cannot connect to SQL Server

**Error**: `Login failed for user 'sa'`

**Solution**:
```bash
# Check SQL Server is running
docker ps | grep sqlserver-test

# Check password is correct
docker exec -it sqlserver-test /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1"
```

### Issue: Database does not exist

**Error**: `Cannot open database "formflow"`

**Solution**:
```bash
# Create database
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "CREATE DATABASE formflow;"

# Run setup script
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -i backend/database/setup.sqlserver.sql
```

### Issue: Tests fail with provider mismatch

**Error**: `InvalidOperationException: Unsupported database provider`

**Solution**:
```bash
# Ensure test configuration is correct
cat backend/tests/FormDesigner.Tests.Integration/appsettings.Test.json

# Or use environment variable
export ASPNETCORE_ENVIRONMENT=Test
export Database__Provider=SqlServer
dotnet test
```

### Issue: Migrations fail

**Error**: `No migrations configuration type was found`

**Solution**:
```bash
# Ensure DesignTimeDbContextFactory is configured
cd backend/src/FormDesigner.API

# Set provider environment variable
export Database__Provider=SqlServer

# Run migrations
dotnet ef database update
```

### Issue: SQL generation produces PostgreSQL syntax

**Error**: Generated SQL contains `TIMESTAMPTZ`, `BOOLEAN`, `STORED`

**Solution**: This is a known issue - SqlGeneratorService needs to be updated to detect provider and generate SQL Server syntax. This will be addressed in the implementation tasks.

## Test Data

Sample test data files can be created in:
- `specs/002-the-system-is/test-data/simple-form.json`
- `specs/002-the-system-is/test-data/table-form.json`
- `specs/002-the-system-is/test-data/complex-form.json`

## Clean Up

### Stop SQL Server Container
```bash
docker stop sqlserver-test
docker rm sqlserver-test
```

### Drop Test Database
```bash
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "DROP DATABASE formflow_test;"
```

### Reset to PostgreSQL
```bash
# Update appsettings.json
sed -i '' 's/"Provider": "SqlServer"/"Provider": "Postgres"/' \
  backend/src/FormDesigner.API/appsettings.json

# Or use environment variable
unset Database__Provider
```

## Next Steps

1. ✅ Complete quickstart guide
2. ➡️ Review `data-model.md` for schema details
3. ➡️ Review `tasks.md` for implementation tasks
4. ➡️ Run integration tests and document results
5. ➡️ Implement SqlGeneratorService updates
6. ➡️ Add SQL Server-specific tests

## Reference

- SQL Server Setup Script: `backend/database/setup.sqlserver.sql`
- Application Config: `backend/src/FormDesigner.API/appsettings.json`
- DbContext: `backend/src/FormDesigner.API/Data/ApplicationDbContext.cs`
- Tests: `backend/tests/FormDesigner.Tests.Integration/`
- Spec: `specs/002-the-system-is/spec.md`
- Research: `specs/002-the-system-is/research.md`
