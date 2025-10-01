using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration test: Create multi-page form with table
/// Maps to Scenario 2 in quickstart.md
/// Tests: Create form with 2 pages, table widget with columns, navigate between pages
/// </summary>
public class MultiPageFormWithTableTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public MultiPageFormWithTableTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Scenario2_CreateMultiPageFormWithTable_ShouldSucceed()
    {
        // Arrange - Create form with 2 pages and table widget
        var formId = "multi-page-inspection-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Multi-Page Inspection Form",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "General Information",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Basic Details",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "inspector-name",
                                        field = new
                                        {
                                            name = "inspector_name",
                                            label = "Inspector Name",
                                            type = "string",
                                            required = true
                                        }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "inspection-date",
                                        field = new
                                        {
                                            name = "inspection_date",
                                            label = "Inspection Date",
                                            type = "date",
                                            required = true
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new
                    {
                        id = "page-2",
                        title = "Inspection Items",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-2",
                                title = "Items Table",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "inspection-table",
                                        title = "Inspection Items",
                                        table = new
                                        {
                                            row_mode = "infinite",
                                            columns = new[]
                                            {
                                                new
                                                {
                                                    name = "item_name",
                                                    label = "Item Name",
                                                    type = "string",
                                                    required = true
                                                },
                                                new
                                                {
                                                    name = "status",
                                                    label = "Status",
                                                    type = "enum",
                                                    @enum = new[] { "pass", "fail", "n/a" },
                                                    required = true
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

        // Act - Step 1: Create the form
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert - Verify creation
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdForm = await createResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        createdForm.Should().NotBeNull();
        createdForm!.Form.Id.Should().Be(formId);
        createdForm.Form.Pages.Should().HaveCount(2);

        // Act - Step 2: Retrieve the form
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");

        // Assert - Verify both pages persisted
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Pages.Should().HaveCount(2);

        var page1 = retrievedForm.Form.Pages[0];
        page1.Id.Should().Be("page-1");
        page1.Title.Should().Be("General Information");

        var page2 = retrievedForm.Form.Pages[1];
        page2.Id.Should().Be("page-2");
        page2.Title.Should().Be("Inspection Items");
    }

    [Fact]
    public async Task Scenario2_TableWithMultipleColumnTypes_ShouldPersist()
    {
        // Arrange - Table with various column types
        var formId = "table-column-types";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Table Column Types Form",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Data Table",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Data",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "data-table",
                                        table = new
                                        {
                                            columns = new[]
                                            {
                                                new { name = "text_col", label = "Text", type = "string" },
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

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - All column types persisted
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
    }

    [Fact]
    public async Task Scenario2_TableWithRowConstraints_ShouldPersist()
    {
        // Arrange - Table with finite row mode and constraints
        var formId = "table-row-constraints";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Row Constraints Table",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Limited Rows",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Constrained Table",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "table",
                                        id = "limited-table",
                                        table = new
                                        {
                                            row_mode = "finite",
                                            min = 1,
                                            max = 10,
                                            row_key = new[] { "item_id" },
                                            columns = new[]
                                            {
                                                new { name = "item_id", label = "ID", type = "string", required = true },
                                                new { name = "value", label = "Value", type = "integer" }
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

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");

        // Assert - Row constraints persisted
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
    }

    [Fact]
    public async Task Scenario2_UpdatePageOrder_ShouldReflectChanges()
    {
        // Arrange - Create form with 2 pages
        var formId = "page-order-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Page Order Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "First Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section 1",
                                widgets = new object[] { }
                            }
                        }
                    },
                    new
                    {
                        id = "page-2",
                        title = "Second Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-2",
                                title = "Section 2",
                                widgets = new object[] { }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Update - Reverse page order
        var updateRequest = new
        {
            form = new
            {
                id = formId,
                title = "Page Order Test",
                version = "2.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-2",
                        title = "Second Page (Now First)",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-2",
                                title = "Section 2",
                                widgets = new object[] { }
                            }
                        }
                    },
                    new
                    {
                        id = "page-1",
                        title = "First Page (Now Second)",
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

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - Page order changed
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Pages[0].Id.Should().Be("page-2");
        retrievedForm.Form.Pages[1].Id.Should().Be("page-1");
    }
}
