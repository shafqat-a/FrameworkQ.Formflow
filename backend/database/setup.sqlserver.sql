-- FormDesigner Database Setup Script
-- Microsoft SQL Server 2019+
-- Creates schema and tables for Form Designer application

IF DB_ID('formflow') IS NULL
BEGIN
    PRINT 'Creating database formflow';
    CREATE DATABASE formflow;
END
GO

USE formflow;
GO

-- ============================================================================
-- TABLE: form_definitions
-- Stores form definitions as JSON payloads
-- ============================================================================
IF OBJECT_ID('dbo.form_definitions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.form_definitions
    (
        form_id        NVARCHAR(100)  NOT NULL CONSTRAINT PK_form_definitions PRIMARY KEY,
        version        NVARCHAR(50)   NOT NULL,
        dsl_json       NVARCHAR(MAX)  NOT NULL,
        is_committed   BIT            NOT NULL CONSTRAINT DF_form_definitions_is_committed DEFAULT (0),
        created_at     DATETIME2      NOT NULL CONSTRAINT DF_form_definitions_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at     DATETIME2      NULL,
        is_active      BIT            NOT NULL CONSTRAINT DF_form_definitions_is_active DEFAULT (1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_definitions_is_active' AND object_id = OBJECT_ID('dbo.form_definitions'))
BEGIN
    CREATE INDEX ix_form_definitions_is_active
        ON dbo.form_definitions (is_active);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_definitions_is_committed' AND object_id = OBJECT_ID('dbo.form_definitions'))
BEGIN
    CREATE INDEX ix_form_definitions_is_committed
        ON dbo.form_definitions (is_committed);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_definitions_created_at' AND object_id = OBJECT_ID('dbo.form_definitions'))
BEGIN
    CREATE INDEX ix_form_definitions_created_at
        ON dbo.form_definitions (created_at DESC);
END
GO

-- ============================================================================
-- TABLE: form_instances
-- Stores runtime submissions
-- ============================================================================
IF OBJECT_ID('dbo.form_instances', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.form_instances
    (
        instance_id   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_form_instances PRIMARY KEY,
        form_id       NVARCHAR(100)    NOT NULL,
        status        NVARCHAR(20)     NOT NULL CONSTRAINT DF_form_instances_status DEFAULT ('draft'),
        data_json     NVARCHAR(MAX)    NULL,
        created_at    DATETIME2        NOT NULL CONSTRAINT DF_form_instances_created_at DEFAULT (SYSUTCDATETIME()),
        submitted_at  DATETIME2        NULL,
        user_id       NVARCHAR(256)    NULL,
        CONSTRAINT FK_form_instances_form_definitions
            FOREIGN KEY (form_id) REFERENCES dbo.form_definitions (form_id)
            ON DELETE NO ACTION
            ON UPDATE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_instances_form_id' AND object_id = OBJECT_ID('dbo.form_instances'))
BEGIN
    CREATE INDEX ix_form_instances_form_id
        ON dbo.form_instances (form_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_instances_status' AND object_id = OBJECT_ID('dbo.form_instances'))
BEGIN
    CREATE INDEX ix_form_instances_status
        ON dbo.form_instances (status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_instances_user_id' AND object_id = OBJECT_ID('dbo.form_instances'))
BEGIN
    CREATE INDEX ix_form_instances_user_id
        ON dbo.form_instances (user_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_form_instances_created_at' AND object_id = OBJECT_ID('dbo.form_instances'))
BEGIN
    CREATE INDEX ix_form_instances_created_at
        ON dbo.form_instances (created_at DESC);
END
GO

-- ============================================================================
-- TABLE: temporary_states
-- Stores in-progress drafts
-- ============================================================================
IF OBJECT_ID('dbo.temporary_states', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.temporary_states
    (
        state_id    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_temporary_states PRIMARY KEY,
        instance_id UNIQUEIDENTIFIER NOT NULL,
        data_json   NVARCHAR(MAX)    NOT NULL,
        saved_at    DATETIME2        NOT NULL CONSTRAINT DF_temporary_states_saved_at DEFAULT (SYSUTCDATETIME()),
        user_id     NVARCHAR(256)    NULL,
        CONSTRAINT FK_temporary_states_form_instances
            FOREIGN KEY (instance_id) REFERENCES dbo.form_instances (instance_id)
            ON DELETE CASCADE
            ON UPDATE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_temporary_states_instance_id' AND object_id = OBJECT_ID('dbo.temporary_states'))
BEGIN
    CREATE INDEX ix_temporary_states_instance_id
        ON dbo.temporary_states (instance_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_temporary_states_saved_at' AND object_id = OBJECT_ID('dbo.temporary_states'))
BEGIN
    CREATE INDEX ix_temporary_states_saved_at
        ON dbo.temporary_states (saved_at DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_temporary_states_user_id' AND object_id = OBJECT_ID('dbo.temporary_states'))
BEGIN
    CREATE INDEX ix_temporary_states_user_id
        ON dbo.temporary_states (user_id);
END
GO

PRINT 'SQL Server schema ensured successfully.';
GO
