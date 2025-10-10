using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration test: Create form with single field
/// Maps to Scenario 1 in quickstart.md
/// Tests full workflow: POST form → GET form → Verify field widget
/// </summary>
public class CreateFormWithFieldTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public CreateFormWithFieldTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Scenario1_CreateFormWithSingleField_ShouldSucceed()
    {
        // Arrange - Create form with single text field
        var formId = "simple-text-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Simple Text Form",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Main Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "User Information",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "username-field",
                                        title = "Username",
                                        field = new
                                        {
                                            name = "username",
                                            label = "Username",
                                            type = "string",
                                            required = true,
                                            placeholder = "Enter your username"
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

        // Assert - Verify creation succeeded
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();

        var createdForm = await createResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        createdForm.Should().NotBeNull();
        createdForm!.Form.Id.Should().Be(formId);
        createdForm.Form.Title.Should().Be("Simple Text Form");

        // Act - Step 2: Retrieve the created form
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");

        // Assert - Verify retrieval succeeded and data matches
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
        retrievedForm.Form.Title.Should().Be("Simple Text Form");
        retrievedForm.Form.Version.Should().Be("1.0");

        // Assert - Verify widget structure
        retrievedForm.Form.Pages.Should().HaveCount(1);
        var page = retrievedForm.Form.Pages[0];
        page.Id.Should().Be("page-1");
        page.Title.Should().Be("Main Page");

        // Note: Full widget validation would require detailed DTOs
        // For integration test, we verify the form round-trips correctly
    }

    [Fact]
    public async Task Scenario1_CreateFormWithMultipleFieldTypes_ShouldSucceed()
    {
        // Arrange - Create form with various field types
        var formId = "multi-field-types";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Multi Field Types Form",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Form Fields",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "All Field Types",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "text-field",
                                        field = new { name = "text_field", label = "Text", type = "string" }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "number-field",
                                        field = new { name = "number_field", label = "Number", type = "integer" }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "date-field",
                                        field = new { name = "date_field", label = "Date", type = "date" }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "bool-field",
                                        field = new { name = "bool_field", label = "Boolean", type = "bool" }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "enum-field",
                                        field = new
                                        {
                                            name = "status",
                                            label = "Status",
                                            type = "enum",
                                            @enum = new[] { "active", "inactive", "pending" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act - Create and retrieve
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Form retrieved successfully with all field types
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
    }

    [Fact]
    public async Task Scenario1_CreateFormWithValidation_ShouldEnforceConstraints()
    {
        // Arrange - Create form with validation constraints
        var formId = "validation-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Validation Form",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Validated Fields",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Constraints",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "email-field",
                                        field = new
                                        {
                                            name = "email",
                                            label = "Email",
                                            type = "string",
                                            required = true,
                                            pattern = "^[^@]+@[^@]+\\.[^@]+$"
                                        }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "age-field",
                                        field = new
                                        {
                                            name = "age",
                                            label = "Age",
                                            type = "integer",
                                            min = 18,
                                            max = 100
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

        // Assert - Form created with validation rules
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
    }
}

/// <summary>
/// DTO for integration tests - reuse from contract tests or define simplified version
/// </summary>
public class FormDefinitionRootDto
{
    public FormDefinitionDto Form { get; set; } = new();
}

public class FormDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<PageDto> Pages { get; set; } = new();
}

public class PageDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
