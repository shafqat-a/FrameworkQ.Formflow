using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for GET /api/export/{id}/sql endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// NOTE: This is a stretch goal feature for generating PostgreSQL DDL from table/grid widgets
/// </summary>
public class ExportApiSqlTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public ExportApiSqlTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task ExportSql_ShouldReturn200_WithSqlContent_WhenFormHasTableWidget()
    {
        // Arrange
        var formId = "sql-export-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "SQL Export Test Form",
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
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "inspection-table",
                                        title = "Inspection Items",
                                        table = new
                                        {
                                            columns = new[]
                                            {
                                                new
                                                {
                                                    name = "item_name",
                                                    label = "Item",
                                                    type = "string"
                                                },
                                                new
                                                {
                                                    name = "status",
                                                    label = "Status",
                                                    type = "enum",
                                                    @enum = new[] { "pass", "fail", "n/a" }
                                                },
                                                new
                                                {
                                                    name = "notes",
                                                    label = "Notes",
                                                    type = "text"
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

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

        // Verify Content-Disposition header
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.DispositionType.Should().Be("attachment");
        contentDisposition.FileName.Should().Contain(formId);
        contentDisposition.FileName.Should().EndWith("_schema.sql");

        // Verify SQL content
        var sqlContent = await response.Content.ReadAsStringAsync();
        sqlContent.Should().NotBeNullOrEmpty();
        sqlContent.Should().Contain("CREATE TABLE").Or.Contain("create table");
        sqlContent.Should().Contain("item_name");
        sqlContent.Should().Contain("status");
        sqlContent.Should().Contain("notes");
    }

    [Fact]
    public async Task ExportSql_ShouldReturn404_WhenFormDoesNotExist()
    {
        // Arrange
        var nonExistentFormId = "non-existent-sql-export";

        // Act
        var response = await _client.GetAsync($"/api/export/{nonExistentFormId}/sql");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportSql_ShouldReturn404_WhenFormIsInactive()
    {
        // Arrange
        var formId = "inactive-sql-export";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Inactive Form",
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
                                widgets = new object[] { }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);
        await _client.DeleteAsync($"/api/forms/{formId}");

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/sql");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportSql_ShouldGenerateCorrectDataTypes_ForAllColumnTypes()
    {
        // Arrange
        var formId = "sql-data-types-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Data Types Test",
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
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "data-types-table",
                                        title = "Data Types",
                                        table = new
                                        {
                                            columns = new[]
                                            {
                                                new { name = "text_col", label = "Text", type = "string" },
                                                new { name = "int_col", label = "Integer", type = "integer" },
                                                new { name = "dec_col", label = "Decimal", type = "decimal" },
                                                new { name = "date_col", label = "Date", type = "date" },
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
        var response = await _client.GetAsync($"/api/export/{formId}/sql");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sqlContent = await response.Content.ReadAsStringAsync();
        sqlContent.Should().Contain("text_col");
        sqlContent.Should().Contain("int_col");
        sqlContent.Should().Contain("dec_col");
        sqlContent.Should().Contain("date_col");
        sqlContent.Should().Contain("bool_col");

        // Verify PostgreSQL data types (case-insensitive)
        sqlContent.ToLowerInvariant().Should().Contain("text").Or.Contain("varchar");
        sqlContent.ToLowerInvariant().Should().Contain("integer").Or.Contain("int");
        sqlContent.ToLowerInvariant().Should().Contain("numeric").Or.Contain("decimal");
        sqlContent.ToLowerInvariant().Should().Contain("date");
        sqlContent.ToLowerInvariant().Should().Contain("boolean").Or.Contain("bool");
    }

    [Theory]
    [InlineData("invalid form id")]
    [InlineData("UPPERCASE")]
    [InlineData("form@123")]
    public async Task ExportSql_ShouldReturn400_WhenIdPatternIsInvalid(string invalidId)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/export/{invalidId}/sql");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSql_ShouldHandleMultipleTableWidgets()
    {
        // Arrange
        var formId = "multiple-tables-sql";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Multiple Tables Form",
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
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "table-1",
                                        title = "First Table",
                                        table = new
                                        {
                                            columns = new[]
                                            {
                                                new { name = "col1", label = "Column 1", type = "string" }
                                            }
                                        }
                                    },
                                    new
                                    {
                                        type = "table",
                                        id = "table-2",
                                        title = "Second Table",
                                        table = new
                                        {
                                            columns = new[]
                                            {
                                                new { name = "col2", label = "Column 2", type = "integer" }
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

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sqlContent = await response.Content.ReadAsStringAsync();

        // Should generate DDL for both tables
        var createTableCount = System.Text.RegularExpressions.Regex.Matches(
            sqlContent,
            "CREATE TABLE",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        ).Count;

        createTableCount.Should().BeGreaterOrEqualTo(2);
        sqlContent.Should().Contain("col1");
        sqlContent.Should().Contain("col2");
    }

    [Fact]
    public async Task ExportSql_ShouldReturn200_EvenWhenNoTableWidgets()
    {
        // Arrange - Form with only field widgets, no tables
        var formId = "no-tables-sql";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "No Tables Form",
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
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "simple-field",
                                        field = new { name = "test", label = "Test", type = "string" }
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

        // Assert
        // Should still return 200, but with empty or comment-only SQL
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sqlContent = await response.Content.ReadAsStringAsync();
        sqlContent.Should().NotBeNull();
        // Either empty or contains a comment about no tables
        if (!string.IsNullOrWhiteSpace(sqlContent))
        {
            sqlContent.Should().Contain("--").Or.Contain("/*").Or.Contain("No table widgets");
        }
    }
}
