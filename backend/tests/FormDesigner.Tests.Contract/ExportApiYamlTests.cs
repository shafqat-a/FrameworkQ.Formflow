using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for GET /api/export/{id}/yaml endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class ExportApiYamlTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public ExportApiYamlTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task ExportYaml_ShouldReturn200_WithYamlContent_WhenFormExists()
    {
        // Arrange
        var formId = "yaml-export-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "YAML Export Test Form",
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

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/x-yaml");

        // Verify Content-Disposition header
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.DispositionType.Should().Be("attachment");
        contentDisposition.FileName.Should().Contain(formId);
        contentDisposition.FileName.Should().EndWith(".yaml");

        // Verify YAML content
        var yamlContent = await response.Content.ReadAsStringAsync();
        yamlContent.Should().NotBeNullOrEmpty();
        yamlContent.Should().Contain("form:");
        yamlContent.Should().Contain($"id: {formId}");
        yamlContent.Should().Contain("title: YAML Export Test Form");
        yamlContent.Should().Contain("version: \"1.0\"").Or.Contain("version: '1.0'");
        yamlContent.Should().Contain("pages:");
    }

    [Fact]
    public async Task ExportYaml_ShouldReturn404_WhenFormDoesNotExist()
    {
        // Arrange
        var nonExistentFormId = "non-existent-yaml-export";

        // Act
        var response = await _client.GetAsync($"/api/export/{nonExistentFormId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportYaml_ShouldReturn404_WhenFormIsInactive()
    {
        // Arrange
        var formId = "inactive-yaml-export";
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
        var response = await _client.GetAsync($"/api/export/{formId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportYaml_ShouldBeValidYaml_WithComplexForm()
    {
        // Arrange
        var formId = "complex-yaml-export";
        var complexForm = new
        {
            form = new
            {
                id = formId,
                title = "Complex YAML Export",
                version = "2.0",
                locale = new[] { "en", "bn" },
                meta = new
                {
                    organization = "Export Test Org",
                    document_no = "EXP-001",
                    tags = new[] { "export", "yaml" }
                },
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Export Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Export Section",
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
                                            required = true
                                        }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "date-field",
                                        title = "Date",
                                        field = new
                                        {
                                            name = "submission_date",
                                            label = "Date",
                                            type = "date"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", complexForm);

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var yamlContent = await response.Content.ReadAsStringAsync();
        yamlContent.Should().Contain("locale:");
        yamlContent.Should().Contain("- en");
        yamlContent.Should().Contain("- bn");
        yamlContent.Should().Contain("meta:");
        yamlContent.Should().Contain("organization: Export Test Org");
        yamlContent.Should().Contain("widgets:");
        yamlContent.Should().Contain("required: true");
    }

    [Theory]
    [InlineData("invalid form id")]
    [InlineData("UPPERCASE")]
    [InlineData("form@123")]
    public async Task ExportYaml_ShouldReturn400_WhenIdPatternIsInvalid(string invalidId)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/export/{invalidId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportYaml_ShouldComplyWithDslV01Specification()
    {
        // Arrange
        var formId = "dsl-v01-compliant-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "DSL v0.1 Compliant Form",
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

        // Act
        var response = await _client.GetAsync($"/api/export/{formId}/yaml");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var yamlContent = await response.Content.ReadAsStringAsync();

        // DSL v0.1 compliance checks
        yamlContent.Should().Contain("form:");  // Root element
        yamlContent.Should().Contain($"id: {formId}");  // ID matches pattern ^[a-z0-9_-]+$
        yamlContent.Should().NotContain("\"form\":");  // Should be clean YAML, not JSON-like

        // Verify structure
        yamlContent.Should().Contain("pages:");
        yamlContent.Should().Contain("sections:");
        yamlContent.Should().Contain("widgets:");
    }
}
