using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for POST /api/forms endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class FormsApiPostCreateTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public FormsApiPostCreateTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task PostForm_ShouldReturn201_WithLocation_WhenValidFormProvided()
    {
        // Arrange
        var requestUri = "/api/forms";
        var validForm = new
        {
            form = new
            {
                id = "safety-inspection-form",
                title = "Safety Inspection Form",
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
                                        title = "Inspector Name",
                                        field = new
                                        {
                                            name = "inspector_name",
                                            label = "Inspector Name",
                                            type = "string",
                                            required = true
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
        var response = await _client.PostAsJsonAsync(requestUri, validForm);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/forms/safety-inspection-form");

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var createdForm = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        createdForm.Should().NotBeNull();
        createdForm!.Form.Should().NotBeNull();
        createdForm.Form.Id.Should().Be("safety-inspection-form");
        createdForm.Form.Title.Should().Be("Safety Inspection Form");
        createdForm.Form.Version.Should().Be("1.0");
        createdForm.Form.Pages.Should().HaveCount(1);
    }

    [Fact]
    public async Task PostForm_ShouldReturn400_WhenFormIdIsInvalid()
    {
        // Arrange
        var requestUri = "/api/forms";
        var invalidForm = new
        {
            form = new
            {
                id = "Invalid Form ID!",  // Contains spaces and special characters
                title = "Invalid Form",
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

        // Act
        var response = await _client.PostAsJsonAsync(requestUri, invalidForm);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Errors.Should().NotBeNull();
        error.Errors.Should().ContainKey("form.id");
    }

    [Fact]
    public async Task PostForm_ShouldReturn400_WhenRequiredFieldsMissing()
    {
        // Arrange
        var requestUri = "/api/forms";
        var incompleteForm = new
        {
            form = new
            {
                id = "incomplete-form",
                // Missing title (required field)
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

        // Act
        var response = await _client.PostAsJsonAsync(requestUri, incompleteForm);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Errors.Should().NotBeNull();
        error.Errors.Should().ContainKey("form.title");
    }

    [Fact]
    public async Task PostForm_ShouldReturn409_WhenFormIdAlreadyExists()
    {
        // Arrange
        var requestUri = "/api/forms";
        var formId = "duplicate-form-test";
        var form = new
        {
            form = new
            {
                id = formId,
                title = "Duplicate Form",
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

        // Create form first time
        await _client.PostAsJsonAsync(requestUri, form);

        // Act - Try to create same form again
        var response = await _client.PostAsJsonAsync(requestUri, form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("already exists").Or.Contain("duplicate");
    }

    [Fact]
    public async Task PostForm_ShouldReturn400_WhenPagesArrayIsEmpty()
    {
        // Arrange
        var requestUri = "/api/forms";
        var formWithoutPages = new
        {
            form = new
            {
                id = "no-pages-form",
                title = "Form Without Pages",
                version = "1.0",
                pages = Array.Empty<object>()  // Empty pages array (minItems: 1 in spec)
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync(requestUri, formWithoutPages);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Errors.Should().NotBeNull();
        error.Errors.Should().ContainKey("form.pages");
    }

    [Fact]
    public async Task PostForm_ShouldReturn201_WithComplexForm()
    {
        // Arrange
        var requestUri = "/api/forms";
        var complexForm = new
        {
            form = new
            {
                id = "complex-inspection-form",
                title = "Complex Inspection Form",
                version = "2.0",
                locale = new[] { "en", "bn" },
                meta = new
                {
                    organization = "Safety Corp",
                    document_no = "DOC-001",
                    effective_date = "2025-01-01",
                    tags = new[] { "safety", "inspection" }
                },
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Inspection Details",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Basic Info",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "date-field",
                                        title = "Inspection Date",
                                        field = new
                                        {
                                            name = "inspection_date",
                                            label = "Date",
                                            type = "date",
                                            required = true
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
        var response = await _client.PostAsJsonAsync(requestUri, complexForm);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdForm = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        createdForm.Should().NotBeNull();
        createdForm!.Form.Id.Should().Be("complex-inspection-form");
        createdForm.Form.Meta.Should().NotBeNull();
        createdForm.Form.Meta!.Organization.Should().Be("Safety Corp");
    }
}

/// <summary>
/// DTO matching FormDefinitionRoot schema from OpenAPI spec
/// </summary>
public class FormDefinitionRootDto
{
    public FormDefinitionDto Form { get; set; } = new();
}

/// <summary>
/// DTO matching FormDefinition schema from OpenAPI spec
/// </summary>
public class FormDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string[]? Locale { get; set; }
    public FormMetadataDto? Meta { get; set; }
    public List<PageDto> Pages { get; set; } = new();
}

/// <summary>
/// DTO matching FormMetadata schema from OpenAPI spec
/// </summary>
public class FormMetadataDto
{
    public string? Organization { get; set; }
    public string? DocumentNo { get; set; }
    public string? EffectiveDate { get; set; }
    public string[]? Tags { get; set; }
}

/// <summary>
/// DTO matching Page schema from OpenAPI spec
/// </summary>
public class PageDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// DTO matching ValidationError schema from OpenAPI spec
/// </summary>
public class ValidationErrorDto
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

/// <summary>
/// DTO matching Error schema from OpenAPI spec
/// </summary>
public class ErrorDto
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
