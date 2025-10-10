# Database Setup

This directory contains database initialization scripts for both PostgreSQL and SQL Server.

## Supported Database Providers

- **PostgreSQL 15+** (Primary/Default)
- **Microsoft SQL Server 2019+** (Fully Supported)

The application is database-independent and can switch providers via configuration only.

## Quick Start

### Option 1: PostgreSQL (Default)

```bash
# Create database
createdb -h localhost -p 5400 -U postgres formflow

# Run setup script
psql -h localhost -p 5400 -U postgres -d formflow -f setup.sql

# Or use EF migrations (recommended)
cd ../src/FormDesigner.API
dotnet ef database update
```

### Option 2: SQL Server

```bash
# Using Docker (recommended for development)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver-formflow \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Create database
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -Q "CREATE DATABASE formflow"

# Option A: Run setup script
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" \
  -d formflow -i setup.sqlserver.sql

# Option B: Use EF migrations (recommended)
cd ../src/FormDesigner.API
export Database__Provider=SqlServer
export ConnectionStrings__FormDesignerDb="Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
dotnet ef database update
```

## Configuration

### Switching Database Providers

Update `appsettings.json` in `backend/src/FormDesigner.API/`:

**PostgreSQL Configuration:**
```json
{
  "Database": {
    "Provider": "Postgres",
    "ConnectionStringName": "FormDesignerDb"
  },
  "ConnectionStrings": {
    "FormDesignerDb": "Host=localhost;Port=5400;Database=formflow;Username=postgres;Password=postgres"
  }
}
```

**SQL Server Configuration:**
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStringName": "FormDesignerDb"
  },
  "ConnectionStrings": {
    "FormDesignerDb": "Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  }
}
```

**Using Environment Variables:**
```bash
# PostgreSQL
export Database__Provider=Postgres
export ConnectionStrings__FormDesignerDb="Host=localhost;Database=formflow;Username=postgres;Password=postgres"

# SQL Server
export Database__Provider=SqlServer
export ConnectionStrings__FormDesignerDb="Server=localhost,1433;Database=formflow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

### Supported Provider Names

Provider names are case-insensitive:
- **PostgreSQL**: `Postgres`, `PostgreSQL`, `Npgsql`
- **SQL Server**: `SqlServer`, `MSSQL`, `MsSql`

## Schema Information

### Tables

1. **form_definitions** - Form design templates (draft and committed)
2. **form_instances** - Runtime form submissions
3. **temporary_states** - Saved progress during data entry
4. **__EFMigrationsHistory** - EF Core migration tracking

### Database-Specific Type Mappings

| Entity Property | PostgreSQL Type | SQL Server Type | Notes |
|----------------|----------------|-----------------|-------|
| FormId (string) | TEXT | NVARCHAR(100) | Primary key |
| Version (string) | TEXT | NVARCHAR(50) | DSL version |
| DslJson (string) | JSONB | NVARCHAR(MAX) | Form definition |
| CreatedAt (DateTime) | TIMESTAMPTZ | DATETIME2 | UTC timestamp |
| UpdatedAt (DateTime?) | TIMESTAMPTZ | DATETIME2 | Nullable |
| IsActive (bool) | BOOLEAN | BIT | Soft delete flag |
| IsCommitted (bool) | BOOLEAN | BIT | Draft vs committed |
| InstanceId (Guid) | UUID | UNIQUEIDENTIFIER | Instance PK |
| Status (string) | TEXT | NVARCHAR(20) | draft/submitted |

### Default Values

| Column | PostgreSQL | SQL Server |
|--------|-----------|------------|
| created_at | NOW() | SYSUTCDATETIME() |
| saved_at | NOW() | SYSUTCDATETIME() |
| is_active | true | 1 |
| is_committed | false | 0 |
| status | 'draft' | 'draft' |

### Indexes

**Shared indexes (both providers):**
- Primary keys: form_id, instance_id, state_id
- Foreign key indexes: form_id (instances), instance_id (states)
- Performance indexes: is_active, is_committed, status, user_id, created_at

**PostgreSQL-specific:**
- GIN indexes on dsl_json and data_json for efficient JSON querying (`@>`, `->`, `->>` operators)

**SQL Server-specific:**
- Full-text indexes can be added for NVARCHAR(MAX) columns if needed

## SQL DDL Generation

The application can generate SQL DDL for table/grid widgets in forms. The generated SQL is provider-specific:

**PostgreSQL:**
- Uses VARCHAR, TEXT, TIMESTAMPTZ, BOOLEAN, UUID
- Computed columns: `GENERATED ALWAYS AS (...) STORED`
- Table creation: `CREATE TABLE IF NOT EXISTS`

**SQL Server:**
- Uses NVARCHAR, DATETIME2, BIT, UNIQUEIDENTIFIER
- Computed columns: `AS (...) PERSISTED`
- Table creation: `IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '...') CREATE TABLE`

## Migrations

EF Core migrations are provider-aware and generate appropriate SQL for the configured database.

### View Pending Migrations

```bash
cd backend/src/FormDesigner.API
dotnet ef migrations list
```

### Create New Migration

```bash
# Set provider first
export Database__Provider=SqlServer  # or Postgres

# Add migration
dotnet ef migrations add MigrationName --verbose
```

### Apply Migrations

```bash
# PostgreSQL
dotnet ef database update

# SQL Server
export Database__Provider=SqlServer
dotnet ef database update --verbose
```

### Generate SQL Script (without applying)

```bash
# Generate SQL for review
dotnet ef migrations script --output migration.sql
```

### Rollback Migration

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Rollback all migrations
dotnet ef database update 0
```

## Testing

Comprehensive integration tests validate both database providers.

```bash
cd backend/tests/FormDesigner.Tests.Integration

# Run SQL Server tests
export SQLSERVER_HOST=localhost,1433
export SQLSERVER_USER=sa
export SQLSERVER_PASSWORD=YourStrong!Passw0rd
dotnet test --filter "FullyQualifiedName~SqlServer"

# Run all integration tests
dotnet test
```

See [SQL Server Testing Guide](../tests/FormDesigner.Tests.Integration/README-SqlServer-Testing.md) for details.

## Troubleshooting

### PostgreSQL Issues

**Connection failed:**
```bash
# Test connection
psql -h localhost -p 5400 -U postgres -c "SELECT version();"

# Check if database exists
psql -U postgres -c "\l" | grep formflow

# Check PostgreSQL status
pg_isready -h localhost -p 5400
```

**Permission denied:**
```sql
GRANT ALL PRIVILEGES ON DATABASE formflow TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;
```

### SQL Server Issues

**Connection failed:**
```bash
# Test connection
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION"

# Docker: Check container status
docker ps | grep sqlserver
docker logs sqlserver-formflow

# Docker: Restart if needed
docker restart sqlserver-formflow
```

**Database does not exist:**
```sql
-- Create database
CREATE DATABASE formflow;
GO
```

**TrustServerCertificate error:**
- Development: Add `TrustServerCertificate=True` to connection string
- Production: Configure proper SSL certificates

### Migration Issues

**"Pending migrations detected":**
```bash
cd backend/src/FormDesigner.API
dotnet ef database update
```

**"Provider not configured":**
- Set `Database:Provider` in appsettings.json or via environment variable
- Verify correct NuGet packages installed

**"Migration already applied":**
- This is normal if running `dotnet ef database update` on an up-to-date database
- Use `dotnet ef migrations list` to see status

## Performance Tuning

### PostgreSQL

```sql
-- Analyze tables for query planning
ANALYZE form_definitions;
ANALYZE form_instances;

-- Rebuild indexes
REINDEX TABLE form_definitions;

-- Vacuum to reclaim space
VACUUM ANALYZE;
```

### SQL Server

```sql
-- Update statistics
UPDATE STATISTICS form_definitions;
UPDATE STATISTICS form_instances;

-- Rebuild indexes
ALTER INDEX ALL ON form_definitions REBUILD;

-- Check index fragmentation
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'DETAILED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10;
```

## Backup and Restore

### PostgreSQL

```bash
# Backup
pg_dump formflow > formflow_backup_$(date +%Y%m%d).sql

# Restore
createdb formflow_restored
psql -d formflow_restored < formflow_backup_20250101.sql
```

### SQL Server

```sql
-- Backup
BACKUP DATABASE formflow
TO DISK = 'C:\Backups\formflow_backup.bak'
WITH FORMAT;

-- Restore
RESTORE DATABASE formflow_restored
FROM DISK = 'C:\Backups\formflow_backup.bak'
WITH MOVE 'formflow' TO 'C:\Data\formflow_restored.mdf',
     MOVE 'formflow_log' TO 'C:\Data\formflow_restored_log.ldf';
```

## Security Best Practices

### Production Deployment

1. **Use secure credentials:**
   - Store connection strings in environment variables or secret managers
   - Never commit passwords to source control
   - Use strong passwords (16+ characters)

2. **Enable encryption:**
   - PostgreSQL: Configure SSL/TLS (`sslmode=require`)
   - SQL Server: Use valid certificates (`TrustServerCertificate=False`)

3. **Limit permissions:**
   - Create application-specific database user (not sa/postgres)
   - Grant only required permissions (SELECT, INSERT, UPDATE, DELETE)
   - No DDL permissions in production

4. **Network security:**
   - Firewall rules to restrict database access
   - Use VPN or private networks
   - Consider read replicas for scaling

### Example Production User

**PostgreSQL:**
```sql
CREATE USER formflow_app WITH PASSWORD 'secure_password_here';
GRANT CONNECT ON DATABASE formflow TO formflow_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO formflow_app;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO formflow_app;
```

**SQL Server:**
```sql
CREATE LOGIN formflow_app WITH PASSWORD = 'Secure_Password_123!';
CREATE USER formflow_app FOR LOGIN formflow_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO formflow_app;
```

## References

- [Setup Script (PostgreSQL)](./setup.sql)
- [Setup Script (SQL Server)](./setup.sqlserver.sql)
- [ApplicationDbContext](../src/FormDesigner.API/Data/ApplicationDbContext.cs) - EF Core configuration
- [SQL Server Testing Guide](../tests/FormDesigner.Tests.Integration/README-SqlServer-Testing.md)
- [Migration Validation Guide](../tests/FormDesigner.Tests.Integration/README-Migration-Validation.md)
- [Feature Specification](../../specs/002-the-system-is/spec.md)
- [Quickstart Guide](../../specs/002-the-system-is/quickstart.md)
