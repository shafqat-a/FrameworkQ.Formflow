using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FormDesigner.API.Data;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests to verify database schema is created correctly for SQL Server provider.
/// Validates tables, columns, data types, indexes, and foreign key constraints.
/// </summary>
public class SchemaValidationTests : IClassFixture<SqlServerTestFixture>
{
    private readonly SqlServerTestFixture _fixture;

    public SchemaValidationTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// TC-101: Verify tables exist
    /// Given: Fresh database with migrations applied
    /// When: Query INFORMATION_SCHEMA.TABLES
    /// Then: form_definitions, form_instances, temporary_states tables exist
    /// </summary>
    [Fact]
    public async Task TC101_SchemaTablesExist_ShouldHaveAllRequiredTables()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act - Query INFORMATION_SCHEMA.TABLES
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            AND TABLE_CATALOG = @databaseName
            ORDER BY TABLE_NAME";
        command.Parameters.AddWithValue("@databaseName", _fixture.DatabaseName);

        var tables = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        // Assert
        tables.Should().Contain("form_definitions", because: "FormDefinitionEntity should create this table");
        tables.Should().Contain("form_instances", because: "FormInstanceEntity should create this table");
        tables.Should().Contain("temporary_states", because: "TemporaryStateEntity should create this table");
    }

    /// <summary>
    /// TC-102: Verify columns and data types for form_definitions table
    /// Given: Database with migrations applied
    /// When: Query INFORMATION_SCHEMA.COLUMNS
    /// Then: All expected columns exist with correct SQL Server data types
    /// </summary>
    [Fact]
    public async Task TC102_SchemaColumns_FormDefinitions_ShouldHaveCorrectDataTypes()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                COLUMN_NAME,
                DATA_TYPE,
                CHARACTER_MAXIMUM_LENGTH,
                IS_NULLABLE,
                COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'form_definitions'
            ORDER BY ORDINAL_POSITION";

        var columns = new Dictionary<string, (string DataType, int? MaxLength, string IsNullable)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var maxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
            var isNullable = reader.GetString(3);
            columns[columnName] = (dataType, maxLength, isNullable);
        }

        // Assert - Verify all columns exist
        columns.Should().ContainKey("form_id");
        columns.Should().ContainKey("version");
        columns.Should().ContainKey("dsl_json");
        columns.Should().ContainKey("is_committed");
        columns.Should().ContainKey("created_at");
        columns.Should().ContainKey("updated_at");
        columns.Should().ContainKey("is_active");

        // Verify data types (SQL Server specific)
        columns["form_id"].DataType.Should().Be("nvarchar");
        columns["form_id"].MaxLength.Should().Be(100);
        columns["form_id"].IsNullable.Should().Be("NO");

        columns["version"].DataType.Should().Be("nvarchar");
        columns["version"].MaxLength.Should().Be(50);
        columns["version"].IsNullable.Should().Be("NO");

        columns["dsl_json"].DataType.Should().Be("nvarchar");
        columns["dsl_json"].MaxLength.Should().Be(-1); // -1 represents MAX
        columns["dsl_json"].IsNullable.Should().Be("NO");

        columns["is_committed"].DataType.Should().Be("bit");
        columns["is_committed"].IsNullable.Should().Be("NO");

        columns["created_at"].DataType.Should().Be("datetime2");
        columns["created_at"].IsNullable.Should().Be("NO");

        columns["updated_at"].DataType.Should().Be("datetime2");
        columns["updated_at"].IsNullable.Should().Be("YES");

        columns["is_active"].DataType.Should().Be("bit");
        columns["is_active"].IsNullable.Should().Be("NO");
    }

    /// <summary>
    /// TC-102b: Verify columns and data types for form_instances table
    /// </summary>
    [Fact]
    public async Task TC102b_SchemaColumns_FormInstances_ShouldHaveCorrectDataTypes()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                COLUMN_NAME,
                DATA_TYPE,
                CHARACTER_MAXIMUM_LENGTH,
                IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'form_instances'
            ORDER BY ORDINAL_POSITION";

        var columns = new Dictionary<string, (string DataType, int? MaxLength, string IsNullable)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var maxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
            var isNullable = reader.GetString(3);
            columns[columnName] = (dataType, maxLength, isNullable);
        }

        // Assert
        columns.Should().ContainKey("instance_id");
        columns["instance_id"].DataType.Should().Be("uniqueidentifier");
        columns["instance_id"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("form_id");
        columns["form_id"].DataType.Should().Be("nvarchar");
        columns["form_id"].MaxLength.Should().Be(100);
        columns["form_id"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("status");
        columns["status"].DataType.Should().Be("nvarchar");
        columns["status"].MaxLength.Should().Be(20);
        columns["status"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("data_json");
        columns["data_json"].DataType.Should().Be("nvarchar");
        columns["data_json"].MaxLength.Should().Be(-1);
        columns["data_json"].IsNullable.Should().Be("YES");

        columns.Should().ContainKey("created_at");
        columns["created_at"].DataType.Should().Be("datetime2");
        columns["created_at"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("submitted_at");
        columns["submitted_at"].DataType.Should().Be("datetime2");
        columns["submitted_at"].IsNullable.Should().Be("YES");

        columns.Should().ContainKey("user_id");
        columns["user_id"].DataType.Should().Be("nvarchar");
        columns["user_id"].MaxLength.Should().Be(256);
        columns["user_id"].IsNullable.Should().Be("YES");
    }

    /// <summary>
    /// TC-102c: Verify columns and data types for temporary_states table
    /// </summary>
    [Fact]
    public async Task TC102c_SchemaColumns_TemporaryStates_ShouldHaveCorrectDataTypes()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                COLUMN_NAME,
                DATA_TYPE,
                CHARACTER_MAXIMUM_LENGTH,
                IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'temporary_states'
            ORDER BY ORDINAL_POSITION";

        var columns = new Dictionary<string, (string DataType, int? MaxLength, string IsNullable)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var maxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
            var isNullable = reader.GetString(3);
            columns[columnName] = (dataType, maxLength, isNullable);
        }

        // Assert
        columns.Should().ContainKey("state_id");
        columns["state_id"].DataType.Should().Be("uniqueidentifier");
        columns["state_id"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("instance_id");
        columns["instance_id"].DataType.Should().Be("uniqueidentifier");
        columns["instance_id"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("data_json");
        columns["data_json"].DataType.Should().Be("nvarchar");
        columns["data_json"].MaxLength.Should().Be(-1);
        columns["data_json"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("saved_at");
        columns["saved_at"].DataType.Should().Be("datetime2");
        columns["saved_at"].IsNullable.Should().Be("NO");

        columns.Should().ContainKey("user_id");
        columns["user_id"].DataType.Should().Be("nvarchar");
        columns["user_id"].MaxLength.Should().Be(256);
        columns["user_id"].IsNullable.Should().Be("YES");
    }

    /// <summary>
    /// TC-103: Verify indexes exist
    /// Given: Database with migrations applied
    /// When: Query index metadata
    /// Then: All non-provider-specific indexes exist
    /// </summary>
    [Fact]
    public async Task TC103_SchemaIndexes_ShouldExistForAllTables()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act - Query sys.indexes for non-primary-key indexes
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                t.name AS TableName,
                i.name AS IndexName,
                i.is_primary_key AS IsPrimaryKey,
                i.is_unique AS IsUnique
            FROM sys.indexes i
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE t.name IN ('form_definitions', 'form_instances', 'temporary_states')
            AND i.type > 0  -- Exclude heaps
            ORDER BY t.name, i.name";

        var indexes = new List<(string TableName, string IndexName, bool IsPrimaryKey, bool IsUnique)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetBoolean(2),
                reader.GetBoolean(3)
            ));
        }

        // Assert - Primary Keys
        indexes.Should().Contain(i => i.TableName == "form_definitions" && i.IsPrimaryKey);
        indexes.Should().Contain(i => i.TableName == "form_instances" && i.IsPrimaryKey);
        indexes.Should().Contain(i => i.TableName == "temporary_states" && i.IsPrimaryKey);

        // Assert - form_definitions indexes
        indexes.Should().Contain(i => i.TableName == "form_definitions" && i.IndexName.Contains("is_active"));
        indexes.Should().Contain(i => i.TableName == "form_definitions" && i.IndexName.Contains("is_committed"));
        indexes.Should().Contain(i => i.TableName == "form_definitions" && i.IndexName.Contains("created_at"));

        // Assert - form_instances indexes
        indexes.Should().Contain(i => i.TableName == "form_instances" && i.IndexName.Contains("form_id"));
        indexes.Should().Contain(i => i.TableName == "form_instances" && i.IndexName.Contains("status"));
        indexes.Should().Contain(i => i.TableName == "form_instances" && i.IndexName.Contains("user_id"));
        indexes.Should().Contain(i => i.TableName == "form_instances" && i.IndexName.Contains("created_at"));

        // Assert - temporary_states indexes
        indexes.Should().Contain(i => i.TableName == "temporary_states" && i.IndexName.Contains("instance_id"));
        indexes.Should().Contain(i => i.TableName == "temporary_states" && i.IndexName.Contains("saved_at"));
        indexes.Should().Contain(i => i.TableName == "temporary_states" && i.IndexName.Contains("user_id"));

        // Note: GIN indexes on JSON columns are PostgreSQL-only, so we don't expect them on SQL Server
    }

    /// <summary>
    /// TC-104: Verify foreign key constraints
    /// Given: Database with migrations applied
    /// When: Query foreign key constraints
    /// Then: form_instances.form_id → form_definitions.form_id (RESTRICT)
    ///       temporary_states.instance_id → form_instances.instance_id (CASCADE)
    /// </summary>
    [Fact]
    public async Task TC104_SchemaForeignKeys_ShouldHaveCorrectConstraints()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                fk.name AS ForeignKeyName,
                tp.name AS ParentTable,
                cp.name AS ParentColumn,
                tr.name AS ReferencedTable,
                cr.name AS ReferencedColumn,
                fk.delete_referential_action_desc AS DeleteAction
            FROM sys.foreign_keys fk
            INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
            INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
            INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
            WHERE tp.name IN ('form_instances', 'temporary_states')
            ORDER BY tp.name, fk.name";

        var foreignKeys = new List<(string ParentTable, string ParentColumn, string ReferencedTable, string ReferencedColumn, string DeleteAction)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            foreignKeys.Add((
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)
            ));
        }

        // Assert - form_instances → form_definitions (RESTRICT/NO_ACTION)
        foreignKeys.Should().Contain(fk =>
            fk.ParentTable == "form_instances" &&
            fk.ParentColumn == "form_id" &&
            fk.ReferencedTable == "form_definitions" &&
            fk.ReferencedColumn == "form_id" &&
            fk.DeleteAction == "NO_ACTION", // SQL Server uses NO_ACTION for RESTRICT
            because: "form_instances should reference form_definitions with RESTRICT delete behavior");

        // Assert - temporary_states → form_instances (CASCADE)
        foreignKeys.Should().Contain(fk =>
            fk.ParentTable == "temporary_states" &&
            fk.ParentColumn == "instance_id" &&
            fk.ReferencedTable == "form_instances" &&
            fk.ReferencedColumn == "instance_id" &&
            fk.DeleteAction == "CASCADE",
            because: "temporary_states should reference form_instances with CASCADE delete behavior");
    }

    /// <summary>
    /// TC-105: Verify default value constraints
    /// Tests that default values are properly configured for columns
    /// </summary>
    [Fact]
    public async Task TC105_SchemaDefaults_ShouldHaveCorrectDefaultValues()
    {
        // Arrange
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        // Act
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                t.name AS TableName,
                c.name AS ColumnName,
                dc.definition AS DefaultValue
            FROM sys.tables t
            INNER JOIN sys.columns c ON t.object_id = c.object_id
            LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
            WHERE t.name IN ('form_definitions', 'form_instances', 'temporary_states')
            AND dc.definition IS NOT NULL
            ORDER BY t.name, c.name";

        var defaults = new Dictionary<(string Table, string Column), string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            var defaultValue = reader.GetString(2);
            defaults[(table, column)] = defaultValue;
        }

        // Assert - form_definitions defaults
        defaults.Should().ContainKey(("form_definitions", "is_committed"));
        defaults[("form_definitions", "is_committed")].Should().Contain("0", because: "is_committed should default to false (0)");

        defaults.Should().ContainKey(("form_definitions", "is_active"));
        defaults[("form_definitions", "is_active")].Should().Contain("1", because: "is_active should default to true (1)");

        defaults.Should().ContainKey(("form_definitions", "created_at"));
        defaults[("form_definitions", "created_at")].ToLower().Should().Contain("sysutcdatetime", because: "created_at should default to SYSUTCDATETIME()");

        // Assert - form_instances defaults
        defaults.Should().ContainKey(("form_instances", "status"));
        defaults[("form_instances", "status")].Should().Contain("draft", because: "status should default to 'draft'");

        defaults.Should().ContainKey(("form_instances", "created_at"));
        defaults[("form_instances", "created_at")].ToLower().Should().Contain("sysutcdatetime", because: "created_at should default to SYSUTCDATETIME()");

        // Assert - temporary_states defaults
        defaults.Should().ContainKey(("temporary_states", "saved_at"));
        defaults[("temporary_states", "saved_at")].ToLower().Should().Contain("sysutcdatetime", because: "saved_at should default to SYSUTCDATETIME()");
    }
}
