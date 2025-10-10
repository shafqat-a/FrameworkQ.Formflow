using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration test: Export YAML and validate DSL compliance
/// Maps to Scenario 3 in quickstart.md
/// Tests: Create form → Export YAML → Parse YAML → Validate structure
/// </summary>
public class YamlExportValidationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly IDeserializer _yamlDeserializer;

    public YamlExportValidationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    [Fact]
    public async Task Scenario3_ExportYaml_ShouldComplyWithDslV01()
    {
        // Arrange - Create a form
        var formId = "yaml-dsl-compliance-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "YAML DSL Compliance Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Test Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Test Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "test-field",
                                        field = new
                                        {
                                            name = "test_field",
                                            label = "Test Field",
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

        // Act - Export YAML
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");

        // Assert - Response headers
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        exportResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/x-yaml");

        var contentDisposition = exportResponse.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.FileName.Should().EndWith(".yaml");

        // Assert - YAML content
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();
        yamlContent.Should().NotBeNullOrEmpty();

        // Parse YAML
        var parsedYaml = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
        parsedYaml.Should().ContainKey("form");

        // Validate structure
        yamlContent.Should().Contain("form:");
        yamlContent.Should().Contain($"id: {formId}");
        yamlContent.Should().Contain("pages:");
        yamlContent.Should().Contain("sections:");
        yamlContent.Should().Contain("widgets:");
    }

    [Fact]
    public async Task Scenario3_YamlExport_ShouldUseSnakeCasePropertyNames()
    {
        // Arrange
        var formId = "snake-case-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Snake Case Test",
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
                                        id = "test-table",
                                        table = new
                                        {
                                            row_mode = "infinite",
                                            columns = new object[]
                                            {
                                                new { name = "column_name", label = "Column", type = "string" }
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
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();

        // Assert - Snake case property names
        yamlContent.Should().Contain("row_mode:");  // Not rowMode or RowMode
        yamlContent.Should().NotContain("rowMode");
        yamlContent.Should().NotContain("RowMode");
    }

    [Fact]
    public async Task Scenario3_YamlExport_ShouldValidateIdPattern()
    {
        // Arrange - Form with valid IDs
        var formId = "valid-id-123";  // Matches ^[a-z0-9_-]+$
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "ID Pattern Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",  // Valid
                        title = "Page",
                        sections = new[]
                        {
                            new
                            {
                                id = "section_1",  // Valid (underscore)
                                title = "Section",
                                widgets = new object[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "field-1",  // Valid (hyphen)
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
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();

        // Assert - All IDs match pattern ^[a-z0-9_-]+$
        var idPattern = new Regex(@"^[a-z0-9_-]+$");

        // Extract IDs from YAML
        var formIdMatch = Regex.Match(yamlContent, @"id:\s+([^\s]+)");
        formIdMatch.Success.Should().BeTrue();
        idPattern.IsMatch(formIdMatch.Groups[1].Value).Should().BeTrue();

        // Verify no uppercase letters in IDs
        yamlContent.Should().NotContainEquivalentOf("ID: Page");
        yamlContent.Should().NotContainEquivalentOf("ID: Section");
    }

    [Fact]
    public async Task Scenario3_YamlExport_ShouldOmitNullValues()
    {
        // Arrange - Form with optional fields not set
        var formId = "null-omit-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Null Omit Test",
                version = "1.0",
                // No locale, labels, meta - these should be omitted in YAML
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
                                        type = "field",
                                        id = "simple-field",
                                        field = new { name = "test", label = "Test", type = "string" }
                                        // No required, placeholder, default - should be omitted
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
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();

        // Assert - Null/absent fields omitted
        yamlContent.Should().NotContain("locale: null");
        yamlContent.Should().NotContain("labels: null");
        yamlContent.Should().NotContain("meta: null");
        yamlContent.Should().NotContain("required: null");
        yamlContent.Should().NotContain("placeholder: null");
    }

    [Fact]
    public async Task Scenario3_YamlExport_ShouldBeValidYamlFormat()
    {
        // Arrange
        var formId = "yaml-format-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "YAML Format Test",
                version = "1.0",
                locale = new[] { "en", "bn" },
                meta = new
                {
                    organization = "Test Org",
                    tags = new[] { "test", "yaml" }
                },
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
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();

        // Assert - Should be parseable as valid YAML
        Action parseAction = () => _yamlDeserializer.Deserialize<object>(yamlContent);
        parseAction.Should().NotThrow("YAML should be valid and parseable");

        // Verify arrays are formatted correctly
        yamlContent.Should().Contain("locale:");
        yamlContent.Should().Contain("- en");
        yamlContent.Should().Contain("- bn");

        // Verify nested objects
        yamlContent.Should().Contain("meta:");
        yamlContent.Should().Contain("organization:");
    }

    [Fact]
    public async Task Scenario3_YamlExport_ShouldHandleSpecialCharactersInStrings()
    {
        // Arrange - Form with special characters that need escaping
        var formId = "special-chars-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Special: Characters & Symbols",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page with \"quotes\"",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Section: Test & Validation",
                                widgets = new object[] { }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act
        var exportResponse = await _client.GetAsync($"/api/export/{formId}/yaml");
        var yamlContent = await exportResponse.Content.ReadAsStringAsync();

        // Assert - Should handle special characters correctly
        Action parseAction = () => _yamlDeserializer.Deserialize<object>(yamlContent);
        parseAction.Should().NotThrow("YAML should handle special characters");

        // Verify content preserved
        yamlContent.Should().Contain("Special");
        yamlContent.Should().Contain("Characters");
        yamlContent.Should().Contain("Symbols");
    }
}
