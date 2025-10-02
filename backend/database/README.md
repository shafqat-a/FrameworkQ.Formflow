# Database Setup

This directory contains database setup scripts for the FormDesigner application.

## Prerequisites

- PostgreSQL 15+ installed and running
- `psql` command-line tool available

## Quick Start

### 1. Create Database

```bash
# Create the formflow database
createdb formflow
```

### 2. Run Setup Script

```bash
# Run setup script
psql -d formflow -f setup.sql
```

### 3. Verify Setup

```bash
# Connect to database
psql -d formflow

# List tables
\dt

# Describe form_definitions table
\d form_definitions

# Check indexes
\di
```

## Connection Strings

Configured in `appsettings.json`:

- **Production & Development**: `Host=localhost;Port=5400;Database=formflow;Username=postgres;Password=orion@123`

## Schema

### form_definitions

Stores form definitions with JSONB DSL representation.

| Column      | Type        | Description                              |
|-------------|-------------|------------------------------------------|
| form_id     | TEXT        | Primary key, matches `^[a-z0-9_-]+$`    |
| version     | TEXT        | DSL version (e.g., "0.1")                |
| dsl_json    | JSONB       | Complete form definition                 |
| created_at  | TIMESTAMPTZ | Creation timestamp                       |
| updated_at  | TIMESTAMPTZ | Last update timestamp (nullable)         |
| is_active   | BOOLEAN     | Soft delete flag (true = active)         |

### Indexes

- `form_definitions_pkey`: Primary key on `form_id`
- `idx_form_definitions_is_active`: Index on `is_active` for filtering
- `idx_form_definitions_created_at`: Index on `created_at` for sorting
- `idx_form_definitions_dsl_json`: GIN index on `dsl_json` for JSONB queries

## Sample Queries

```sql
-- Get all active forms
SELECT form_id, version, created_at
FROM form_definitions
WHERE is_active = true
ORDER BY created_at DESC;

-- Query JSONB fields
SELECT form_id, dsl_json->>'metadata'->>'title' as title
FROM form_definitions
WHERE dsl_json @> '{"version": "0.1"}';

-- Soft delete a form
UPDATE form_definitions
SET is_active = false, updated_at = NOW()
WHERE form_id = 'sample-form';
```

## Maintenance

```bash
# Backup database
pg_dump formflow > backup.sql

# Restore database
psql -d formflow < backup.sql

# Vacuum and analyze
psql -d formflow -c "VACUUM ANALYZE form_definitions;"
```

## Troubleshooting

### Connection Issues

If you cannot connect to PostgreSQL:

1. Check PostgreSQL is running: `pg_isready`
2. Verify connection settings in `appsettings.json`
3. Ensure PostgreSQL is listening on port 5400
4. Check PostgreSQL logs

### Permission Issues

If you get permission errors:

```sql
-- Grant permissions to user
GRANT ALL PRIVILEGES ON DATABASE formflow TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
```
