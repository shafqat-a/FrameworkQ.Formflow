using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests to verify SQL DDL generation produces valid SQL Server syntax.
/// Validates that SqlGeneratorService generates correct SQL Server DDL statements.
/// </summary>
public class SqlServerGenerationTests : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;

    public SqlServerGenerationTests(SqlServerTestFixture fixture)
    {
        _client = fixture.Client;
    }

    /// <summary>
    /// TC-502: Generate SQL for SQL Server
    /// Given: Form with table widget, SQL Server provider active
    /// When: GET /api/export/{formId}/sql
    /// Then: SQL contains SQL Server-specific types (NVARCHAR, DATETIME2, BIT)
    ///       CREATE TABLE syntax valid for SQL Server
    ///       Computed columns use PERSISTED keyword
    /// </summary>
    [Fact]
    public async Task TC502_GenerateSqlForSqlServer_ShouldUseSqlServerTypes()
    {
        // Arrange - Create form with table widget
        var formId = $"test-sql-gen-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "SQL Generation Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Data Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Data Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "data-table",
                                        title = "Data Table",
                                        table = new
                                        {
                                            columns = new object[]
                                            {
                                                new { name = "item_name", label = "Item", type = "string" },
                                                new { name = "quantity", label = "Quantity", type = "integer" },
                                                new { name = "price", label = "Price", type = "decimal" },
                                                new { name = "in_stock", label = "In Stock", type = "bool" },
                                                new { name = "created_date", label = "Created", type = "datetime" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act - Generate SQL
        var response = await _client.GetAsync($"/api/export/{formId}/sql");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "SQL generation should succeed");

        var sql = await response.Content.ReadAsStringAsync();
        sql.Should().NotBeNullOrEmpty();

        // Verify SQL Server-specific types
        sql.Should().Contain("NVARCHAR", because: "SQL Server uses NVARCHAR for strings");
        sql.Should().Contain("INT", because: "SQL Server uses INT for integers");
        sql.Should().Contain("DECIMAL", because: "SQL Server uses DECIMAL for decimals");
        sql.Should().Contain("BIT", because: "SQL Server uses BIT for booleans");
        sql.Should().Contain("DATETIME2", because: "SQL Server uses DATETIME2 for timestamps");
        sql.Should().Contain("UNIQUEIDENTIFIER", because: "SQL Server uses UNIQUEIDENTIFIER for GUIDs");

        // Verify SQL Server-specific syntax
        sql.Should().Contain("IF NOT EXISTS", because: "SQL Server uses IF NOT EXISTS for conditional table creation");
        sql.Should().Contain("SYSUTCDATETIME()", because: "SQL Server uses SYSUTCDATETIME() for UTC timestamps");

        // Should NOT contain PostgreSQL-specific syntax
        sql.Should().NotContain("VARCHAR(", because: "SQL Server should use NVARCHAR");
        sql.Should().NotContain("TEXT", because: "SQL Server should use NVARCHAR(MAX)");
        sql.Should().NotContain("BOOLEAN", because: "SQL Server should use BIT");
        sql.Should().NotContain("TIMESTAMPTZ", because: "SQL Server should use DATETIME2");
        sql.Should().NotContain("UUID", because: "SQL Server should use UNIQUEIDENTIFIER");
        sql.Should().NotContain("NOW()", because: "SQL Server should use SYSUTCDATETIME()");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-503: Type mapping correctness
    /// Given: Table widget with all supported data types
    /// When: Generate SQL DDL
    /// Then: Each DSL type maps correctly to SQL Server type
    /// </summary>
    [Fact]
    public async Task TC503_TypeMappingCorrectness_ShouldMapAllTypesCorrectly()
    {
        // Arrange - Form with all data types
        var formId = $"test-types-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Type Mapping Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Types Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Types Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "type-test-table",
                                        table = new
                                        {
                                            columns = new object[]
                                            {
                                                new { name = "text_col", label = "Text", type = "string" },
                                                new { name = "long_text", label = "Long Text", type = "text" },
                                                new { name = "int_col", label = "Integer", type = "integer" },
                                                new { name = "dec_col", label = "Decimal", type = "decimal" },
                                                new { name = "date_col", label = "Date", type = "date" },
                                                new { name = "time_col", label = "Time", type = "time" },
                                                new { name = "datetime_col", label = "DateTime", type = "datetime" },
                                                new { name = "bool_col", label = "Boolean", type = "bool" },
                                                new { name = "enum_col", label = "Enum", type = "enum" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert - Verify each type mapping
        var typeMappings = new Dictionary<string, string>
        {
            ["text_col"] = "NVARCHAR(255)",
            ["long_text"] = "NVARCHAR(MAX)",
            ["int_col"] = "INT",
            ["dec_col"] = "DECIMAL(18,2)",
            ["date_col"] = "DATE",
            ["time_col"] = "TIME",
            ["datetime_col"] = "DATETIME2",
            ["bool_col"] = "BIT",
            ["enum_col"] = "NVARCHAR(100)"
        };

        foreach (var (columnName, expectedType) in typeMappings)
        {
            sql.Should().Contain($"{columnName} {expectedType}",
                because: $"{columnName} should map to {expectedType} in SQL Server");
        }

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-504: Computed column syntax
    /// Given: Table with computed column (formula: "quantity * unit_price")
    /// When: Generate SQL DDL
    /// Then: SQL Server uses "AS (...) PERSISTED" syntax
    /// </summary>
    [Fact]
    public async Task TC504_ComputedColumnSyntax_ShouldUsePersisted()
    {
        // Arrange - Form with computed column
        var formId = $"test-computed-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Computed Column Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Computed Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Computed Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "computed-table",
                                        table = new
                                        {
                                            columns = new object[]
                                            {
                                                new { name = "quantity", label = "Quantity", type = "integer" },
                                                new { name = "unit_price", label = "Unit Price", type = "decimal" },
                                                new
                                                {
                                                    name = "total_price",
                                                    label = "Total",
                                                    type = "decimal",
                                                    formula = "quantity * unit_price"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert - Verify SQL Server computed column syntax
        sql.Should().Contain("AS (", because: "SQL Server uses AS for computed columns");
        sql.Should().Contain("PERSISTED", because: "SQL Server uses PERSISTED keyword for computed columns");
        sql.Should().Contain("total_price", because: "computed column should be included");

        // Should NOT contain PostgreSQL syntax
        sql.Should().NotContain("GENERATED ALWAYS AS", because: "that's PostgreSQL syntax");
        sql.Should().NotContain("STORED", because: "SQL Server uses PERSISTED instead of STORED");

        // Verify formula is present
        sql.Should().Contain("quantity", because: "formula should reference quantity");
        sql.Should().Contain("unit_price", because: "formula should reference unit_price");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-505: Index generation for SQL Server
    /// Verifies that indexes are generated correctly
    /// </summary>
    [Fact]
    public async Task TC505_IndexGeneration_ShouldGenerateCorrectIndexes()
    {
        // Arrange
        var formId = $"test-indexes-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Index Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Index Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Index Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "indexed-table",
                                        table = new
                                        {
                                            columns = new object[]
                                            {
                                                new { name = "id", label = "ID", type = "string" },
                                                new { name = "status", label = "Status", type = "string" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert
        sql.Should().Contain("CREATE INDEX", because: "indexes should be generated");
        sql.Should().Contain("instance_id", because: "instance_id index should be created");
        sql.Should().Contain("recorded_at", because: "recorded_at index should be created");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-506: Primary key generation
    /// Verifies correct primary key syntax
    /// </summary>
    [Fact]
    public async Task TC506_PrimaryKeyGeneration_ShouldGenerateCorrectly()
    {
        // Arrange
        var formId = $"test-pk-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Primary Key Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "PK Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "PK Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "pk-table",
                                        table = new
                                        {
                                            columns = new object[]
                                            {
                                                new { name = "col1", label = "Col1", type = "string" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert
        sql.Should().Contain("PRIMARY KEY", because: "primary key should be defined");
        sql.Should().Contain("instance_id", because: "instance_id is part of primary key");
        sql.Should().Contain("row_id", because: "row_id is part of primary key");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-507: Grid widget SQL generation
    /// Verifies that grid widgets generate correct SQL
    /// </summary>
    [Fact]
    public async Task TC507_GridWidgetGeneration_ShouldGenerateCorrectly()
    {
        // Arrange
        var formId = $"test-grid-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Grid Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Grid Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Grid Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "grid",
                                        id = "test-grid",
                                        grid = new
                                        {
                                            cell = new { type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert
        sql.Should().Contain("row_key", because: "grid tables have row_key");
        sql.Should().Contain("column_key", because: "grid tables have column_key");
        sql.Should().Contain("cell_value", because: "grid tables have cell_value");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-508: Multiple tables in one form
    /// Verifies handling of multiple table/grid widgets
    /// </summary>
    [Fact]
    public async Task TC508_MultipleTables_ShouldGenerateAll()
    {
        // Arrange
        var formId = $"test-multi-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Multiple Tables Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Multi Page",
                        sections = new object[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Multi Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "table-1",
                                        table = new
                                        {
                                            columns = new object[] { new { name = "col1", label = "Col1", type = "string" } }
                                        }
                                    },
                                    new
                                    {
                                        type = "table",
                                        id = "table-2",
                                        table = new
                                        {
                                            columns = new object[] { new { name = "col2", label = "Col2", type = "integer" } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");
        var sql = await response.Content.ReadAsStringAsync();

        // Assert
        sql.Should().Contain("table-1", because: "first table should be generated");
        sql.Should().Contain("table-2", because: "second table should be generated");
        sql.Should().Contain("col1", because: "first table column should be present");
        sql.Should().Contain("col2", because: "second table column should be present");

        // Should have multiple CREATE TABLE statements
        var createTableCount = System.Text.RegularExpressions.Regex.Matches(sql, "CREATE TABLE").Count;
        createTableCount.Should().BeGreaterOrEqualTo(2, because: "should have at least 2 CREATE TABLE statements");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }
}
