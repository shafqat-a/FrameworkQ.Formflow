# Database Setup

This directory contains database setup scripts for the FormDesigner application.

## Prerequisites

- PostgreSQL 15+ installed and running
- `psql` command-line tool available

## Quick Start

### 1. Create Database

```bash
# Create production database
createdb formdesigner

# Create development database
createdb formdesigner_dev
```

### 2. Run Setup Script

```bash
# Production database
psql -d formdesigner -f setup.sql

# Development database
psql -d formdesigner_dev -f setup.sql
```

### 3. Verify Setup

```bash
# Connect to database
psql -d formdesigner_dev

# List tables
\dt

# Describe form_definitions table
\d form_definitions

# Check indexes
\di
```

## Connection Strings

Configured in `appsettings.json`:

- **Production**: `Host=localhost;Port=5432;Database=formdesigner;Username=postgres;Password=postgres`
- **Development**: `Host=localhost;Port=5432;Database=formdesigner_dev;Username=postgres;Password=postgres`

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
pg_dump formdesigner_dev > backup.sql

# Restore database
psql -d formdesigner_dev < backup.sql

# Vacuum and analyze
psql -d formdesigner_dev -c "VACUUM ANALYZE form_definitions;"
```

## Troubleshooting

### Connection Issues

If you cannot connect to PostgreSQL:

1. Check PostgreSQL is running: `pg_isready`
2. Verify connection settings in `appsettings.json`
3. Check PostgreSQL logs: `tail -f /usr/local/var/log/postgres.log` (macOS Homebrew)

### Permission Issues

If you get permission errors:

```sql
-- Grant permissions to user
GRANT ALL PRIVILEGES ON DATABASE formdesigner_dev TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
```
