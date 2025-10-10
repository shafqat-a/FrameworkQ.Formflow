using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Integration test: Form validation
/// Maps to Scenario 6 in quickstart.md
/// Tests: Invalid forms rejected with appropriate error messages
/// </summary>
public class FormValidationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public FormValidationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Scenario6_InvalidFormId_ShouldBeRejected()
    {
        // Arrange - Form with uppercase ID (invalid pattern)
        var createRequest = new
        {
            form = new
            {
                id = "INVALID-UPPERCASE-ID",  // Should be lowercase
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
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().ContainKey("form.id");
        error.Errors["form.id"].Should().Contain(msg =>
            msg.Contains("pattern") || msg.Contains("lowercase") || msg.Contains("^[a-z0-9_-]+$"));
    }

    [Theory]
    [InlineData("invalid ID with spaces")]
    [InlineData("invalid@special#chars")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Scenario6_InvalidFormIdPatterns_ShouldBeRejected(string invalidId)
    {
        // Arrange
        var createRequest = new
        {
            form = new
            {
                id = invalidId,
                title = "Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new { id = "section-1", title = "Section 1", widgets = new object[] { } }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Scenario6_InvalidWidgetId_ShouldBeRejected()
    {
        // Arrange - Widget with invalid ID
        var createRequest = new
        {
            form = new
            {
                id = "valid-form-id",
                title = "Test Form",
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
                                        type = "field",
                                        id = "INVALID WIDGET ID",  // Spaces and uppercase
                                        field = new { name = "test", label = "Test", type = "string" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Keys.Should().Contain(key => key.Contains("widget") && key.Contains("id"));
    }

    [Fact]
    public async Task Scenario6_EmptyFormId_ShouldBeRejected()
    {
        // Arrange
        var createRequest = new
        {
            form = new
            {
                id = "",  // Empty ID
                title = "Test",
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new { id = "section-1", title = "Section 1", widgets = new object[] { } }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().ContainKey("form.id");
    }

    [Fact]
    public async Task Scenario6_DuplicateWidgetIds_ShouldBeRejected()
    {
        // Arrange - Two widgets with same ID
        var createRequest = new
        {
            form = new
            {
                id = "duplicate-widget-test",
                title = "Duplicate Widget Test",
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
                                        type = "field",
                                        id = "duplicate-id",  // Same ID
                                        field = new { name = "field1", label = "Field 1", type = "string" }
                                    },
                                    new
                                    {
                                        type = "field",
                                        id = "duplicate-id",  // Same ID
                                        field = new { name = "field2", label = "Field 2", type = "string" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Keys.Should().Contain(key =>
            key.Contains("widget") && (key.Contains("duplicate") || key.Contains("unique")));
    }

    [Fact]
    public async Task Scenario6_MissingRequiredFields_ShouldBeRejected()
    {
        // Arrange - Missing required 'title' field
        var createRequest = new
        {
            form = new
            {
                id = "missing-title",
                // title is missing
                version = "1.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        sections = new[]
                        {
                            new { id = "section-1", title = "Section 1", widgets = new object[] { } }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().ContainKey("form.title");
    }

    [Fact]
    public async Task Scenario6_EmptyPagesArray_ShouldBeRejected()
    {
        // Arrange - No pages (minItems: 1 in spec)
        var createRequest = new
        {
            form = new
            {
                id = "no-pages",
                title = "No Pages",
                version = "1.0",
                pages = Array.Empty<object>()
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().ContainKey("form.pages");
    }

    [Fact]
    public async Task Scenario6_InvalidDataType_ShouldBeRejected()
    {
        // Arrange - Invalid field type
        var createRequest = new
        {
            form = new
            {
                id = "invalid-data-type",
                title = "Invalid Data Type",
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
                                        type = "field",
                                        id = "invalid-type-field",
                                        field = new
                                        {
                                            name = "test",
                                            label = "Test",
                                            type = "invalid_type"  // Not in enum
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
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task Scenario6_ValidForm_ShouldBeAccepted()
    {
        // Arrange - Completely valid form
        var createRequest = new
        {
            form = new
            {
                id = "valid-form-test",
                title = "Valid Form",
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
                                        type = "field",
                                        id = "valid-field",
                                        field = new { name = "test", label = "Test", type = "string" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert - Should succeed
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

public class ValidationErrorDto
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
