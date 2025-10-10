using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration test: Generate SQL DDL
/// Maps to Scenario 4 in quickstart.md
/// Tests: Create form with table widget → Generate SQL → Verify DDL
/// </summary>
public class SqlGenerationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public SqlGenerationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Scenario4_GenerateSqlDdl_ShouldCreateTableStatement()
    {
        // Arrange - Create form with table widget
        var formId = "sql-generation-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "SQL Generation Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Data Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Data Table",
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
                                                new { name = "price", label = "Price", type = "decimal" }
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
        var sqlResponse = await _client.GetAsync($"/api/export/{formId}/sql");

        // Assert - Response headers
        sqlResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        sqlResponse.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

        var contentDisposition = sqlResponse.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.FileName.Should().EndWith("_schema.sql");

        // Assert - SQL content
        var sqlContent = await sqlResponse.Content.ReadAsStringAsync();
        sqlContent.Should().NotBeNullOrEmpty();

        // Verify CREATE TABLE statement
        sqlContent.ToLowerInvariant().Should().Contain("create table");

        // Verify standard columns
        sqlContent.Should().Contain("instance_id");
        sqlContent.Should().Contain("page_id");
        sqlContent.Should().Contain("section_id");
        sqlContent.Should().Contain("widget_id");
        sqlContent.Should().Contain("row_id");
        sqlContent.Should().Contain("recorded_at");

        // Verify data columns
        sqlContent.Should().Contain("item_name");
        sqlContent.Should().Contain("quantity");
        sqlContent.Should().Contain("price");
    }

    [Fact]
    public async Task Scenario4_GenerateSqlDdl_ShouldMapDataTypesCorrectly()
    {
        // Arrange - Table with various data types
        var formId = "sql-data-types";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "SQL Data Types",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section 1",
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
                                                new { name = "bool_col", label = "Boolean", type = "bool" }
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
        var sqlResponse = await _client.GetAsync($"/api/export/{formId}/sql");
        var sqlContent = await sqlResponse.Content.ReadAsStringAsync();

        // Assert - PostgreSQL type mapping
        var lowerSql = sqlContent.ToLowerInvariant();

        // String types
        lowerSql.Should().ContainAny("varchar", "text");

        // Numeric types
        lowerSql.Should().ContainAny("integer", "int");
        lowerSql.Should().ContainAny("numeric", "decimal");

        // Date/time types
        lowerSql.Should().Contain("date");
        lowerSql.Should().Contain("time");
        lowerSql.Should().ContainAny("timestamp", "datetime");

        // Boolean type
        lowerSql.Should().ContainAny("boolean", "bool");
    }

    [Fact]
    public async Task Scenario4_GenerateSqlDdl_ShouldIncludeComputedColumns()
    {
        // Arrange - Table with computed column
        var formId = "sql-computed-column";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Computed Column Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section 1",
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
        var sqlResponse = await _client.GetAsync($"/api/export/{formId}/sql");
        var sqlContent = await sqlResponse.Content.ReadAsStringAsync();

        // Assert - GENERATED ALWAYS AS for computed column
        sqlContent.Should().Contain("total_price");
        sqlContent.ToUpperInvariant().Should().Contain("GENERATED ALWAYS AS");

        // Should translate formula to SQL expression
        sqlContent.Should().Contain("quantity");
        sqlContent.Should().Contain("unit_price");
        sqlContent.Should().Contain("*");  // Multiplication operator
    }

    [Fact]
    public async Task Scenario4_GenerateSqlDdl_ShouldCreateIndexes()
    {
        // Arrange
        var formId = "sql-indexes-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Indexes Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section 1",
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
        var sqlResponse = await _client.GetAsync($"/api/export/{formId}/sql");
        var sqlContent = await sqlResponse.Content.ReadAsStringAsync();

        // Assert - CREATE INDEX statements
        sqlContent.ToUpperInvariant().Should().Contain("CREATE INDEX");

        // Verify indexes on standard columns
        var indexMatches = Regex.Matches(sqlContent, @"CREATE\s+INDEX", RegexOptions.IgnoreCase);
        indexMatches.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Scenario4_GenerateSqlDdl_ShouldHandleMultipleTables()
    {
        // Arrange - Form with multiple table widgets
        var formId = "sql-multiple-tables";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Multiple Tables",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section 1",
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
        var sqlResponse = await _client.GetAsync($"/api/export/{formId}/sql");
        var sqlContent = await sqlResponse.Content.ReadAsStringAsync();

        // Assert - Multiple CREATE TABLE statements
        var createTableMatches = Regex.Matches(sqlContent, @"CREATE\s+TABLE", RegexOptions.IgnoreCase);
        createTableMatches.Count.Should().BeGreaterOrEqualTo(2);

        sqlContent.Should().Contain("col1");
        sqlContent.Should().Contain("col2");
    }
}
