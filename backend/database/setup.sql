-- FormDesigner Database Setup Script
-- PostgreSQL 15+
-- Creates schema and tables for Form Designer application

-- Drop existing schema if needed (uncomment for fresh setup)
-- DROP SCHEMA IF EXISTS public CASCADE;
-- CREATE SCHEMA public;

-- Enable JSONB support (should be enabled by default in PostgreSQL 9.4+)
-- Verify with: SELECT version();

-- ============================================================================
-- TABLE: form_definitions
-- Stores form definitions with JSONB DSL representation
-- ============================================================================

CREATE TABLE IF NOT EXISTS form_definitions (
    form_id TEXT PRIMARY KEY,
    version TEXT NOT NULL,
    dsl_json JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT form_id_pattern CHECK (form_id ~ '^[a-z0-9_-]+$')
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_form_definitions_is_active
    ON form_definitions(is_active);

CREATE INDEX IF NOT EXISTS idx_form_definitions_created_at
    ON form_definitions(created_at DESC);

-- GIN index for JSONB queries
CREATE INDEX IF NOT EXISTS idx_form_definitions_dsl_json
    ON form_definitions USING GIN(dsl_json);

-- ============================================================================
-- COMMENTS
-- ============================================================================

COMMENT ON TABLE form_definitions IS 'Stores form definitions with DSL v0.1 JSONB representation';
COMMENT ON COLUMN form_definitions.form_id IS 'Unique form identifier matching ^[a-z0-9_-]+$ pattern';
COMMENT ON COLUMN form_definitions.version IS 'DSL version (e.g., 0.1)';
COMMENT ON COLUMN form_definitions.dsl_json IS 'Complete form definition in JSONB format';
COMMENT ON COLUMN form_definitions.created_at IS 'Timestamp when form was created';
COMMENT ON COLUMN form_definitions.updated_at IS 'Timestamp when form was last updated';
COMMENT ON COLUMN form_definitions.is_active IS 'Soft delete flag - false means deleted';

-- ============================================================================
-- SAMPLE DATA (Optional - for development/testing)
-- Uncomment to insert sample form
-- ============================================================================

/*
INSERT INTO form_definitions (form_id, version, dsl_json, created_at, is_active)
VALUES (
    'sample-contact-form',
    '0.1',
    '{
        "id": "sample-contact-form",
        "version": "0.1",
        "metadata": {
            "title": "Contact Form",
            "description": "Sample contact form"
        },
        "widgets": [
            {
                "id": "name-input",
                "type": "textbox",
                "label": "Name",
                "required": true,
                "validation": {
                    "minLength": 2,
                    "maxLength": 100
                }
            },
            {
                "id": "email-input",
                "type": "textbox",
                "label": "Email",
                "required": true,
                "validation": {
                    "pattern": "^[^@]+@[^@]+\\.[^@]+$"
                }
            }
        ]
    }'::JSONB,
    NOW(),
    true
)
ON CONFLICT (form_id) DO NOTHING;
*/

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Verify table creation
SELECT
    table_name,
    table_type
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name = 'form_definitions';

-- Verify indexes
SELECT
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'form_definitions';

-- Show row count
SELECT COUNT(*) as total_forms FROM form_definitions;

-- ============================================================================
-- MAINTENANCE
-- ============================================================================

-- Vacuum analyze for performance optimization
VACUUM ANALYZE form_definitions;

COMMIT;
