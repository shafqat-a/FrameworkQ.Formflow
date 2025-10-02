using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for GET /api/forms/{id} endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class FormsApiGetByIdTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public FormsApiGetByIdTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetFormById_ShouldReturn200_WithFormDefinition_WhenFormExists()
    {
        // Arrange
        var formId = "test-get-by-id";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Test Form for Get By ID",
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
                                        id = "test-field",
                                        title = "Test Field",
                                        field = new
                                        {
                                            name = "test_field",
                                            label = "Test",
                                            type = "string"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Create form first
        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var form = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        form.Should().NotBeNull();
        form!.Form.Should().NotBeNull();
        form.Form.Id.Should().Be(formId);
        form.Form.Title.Should().Be("Test Form for Get By ID");
        form.Form.Version.Should().Be("1.0");
        form.Form.Pages.Should().HaveCount(1);
        form.Form.Pages[0].Id.Should().Be("page-1");
    }

    [Fact]
    public async Task GetFormById_ShouldReturn404_WhenFormDoesNotExist()
    {
        // Arrange
        var nonExistentFormId = "non-existent-form-12345";
        var requestUri = $"/api/forms/{nonExistentFormId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Message.Should().Contain("not found").Or.Contain("does not exist");
    }

    [Fact]
    public async Task GetFormById_ShouldReturn404_WhenFormIsInactive()
    {
        // Arrange
        var formId = "test-inactive-form";
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

        // Create and delete form
        await _client.PostAsJsonAsync("/api/forms", createRequest);
        await _client.DeleteAsync($"/api/forms/{formId}");

        // Act
        var response = await _client.GetAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("invalid form id")]   // Contains spaces
    [InlineData("UPPERCASE-FORM")]     // Contains uppercase
    [InlineData("form@special")]       // Contains special character
    [InlineData("form#123")]           // Contains hash
    public async Task GetFormById_ShouldReturn400_WhenFormIdPatternIsInvalid(string invalidId)
    {
        // Arrange
        var requestUri = $"/api/forms/{invalidId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("pattern").Or.Contain("invalid");
    }

    [Fact]
    public async Task GetFormById_ShouldReturnCompleteFormStructure()
    {
        // Arrange
        var formId = "complete-structure-form";
        var complexForm = new
        {
            form = new
            {
                id = formId,
                title = "Complete Structure Form",
                version = "2.5",
                locale = new[] { "en", "bn" },
                labels = new Dictionary<string, string>
                {
                    { "submit", "Submit Form" },
                    { "cancel", "Cancel" }
                },
                meta = new
                {
                    organization = "Test Org",
                    document_no = "DOC-002",
                    effective_date = "2025-01-15",
                    revision_no = "Rev 1",
                    tags = new[] { "test", "complete" }
                },
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page One",
                        labels = new Dictionary<string, string>
                        {
                            { "next", "Next Page" }
                        },
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "First Section",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "name-field",
                                        title = "Name",
                                        field = new
                                        {
                                            name = "user_name",
                                            label = "Full Name",
                                            type = "string",
                                            required = true,
                                            placeholder = "Enter your name"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Create form
        await _client.PostAsJsonAsync("/api/forms", complexForm);

        // Act
        var response = await _client.GetAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var form = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        form.Should().NotBeNull();
        form!.Form.Id.Should().Be(formId);
        form.Form.Version.Should().Be("2.5");
        form.Form.Locale.Should().BeEquivalentTo(new[] { "en", "bn" });
        form.Form.Meta.Should().NotBeNull();
        form.Form.Meta!.Organization.Should().Be("Test Org");
        form.Form.Meta.Tags.Should().BeEquivalentTo(new[] { "test", "complete" });
    }
}
