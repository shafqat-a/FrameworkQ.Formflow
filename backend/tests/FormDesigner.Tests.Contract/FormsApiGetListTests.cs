using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for GET /api/forms endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class FormsApiGetListTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public FormsApiGetListTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetForms_ShouldReturn200_WithEmptyArray_WhenNoFormsExist()
    {
        // Arrange
        var requestUri = "/api/forms";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var forms = await response.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        forms.Should().NotBeNull();
        forms.Should().BeEmpty();
    }

    [Fact]
    public async Task GetForms_ShouldReturn200_WithFormSummaryArray_WhenFormsExist()
    {
        // Arrange
        var requestUri = "/api/forms";

        // Create a test form first
        var createRequest = new
        {
            form = new
            {
                id = "test-form-list",
                title = "Test Form for List",
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
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var forms = await response.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        forms.Should().NotBeNull();
        forms.Should().NotBeEmpty();
        forms.Should().ContainSingle(f => f.FormId == "test-form-list");

        var form = forms!.First(f => f.FormId == "test-form-list");
        form.Version.Should().Be("1.0");
        form.Title.Should().Be("Test Form for List");
        form.IsActive.Should().BeTrue();
        form.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetForms_ShouldReturnOnlyActiveForms_ByDefault()
    {
        // Arrange
        var activeFormId = "active-form-123";
        var inactiveFormId = "inactive-form-456";

        // Create active form
        var activeForm = new
        {
            form = new
            {
                id = activeFormId,
                title = "Active Form",
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

        await _client.PostAsJsonAsync("/api/forms", activeForm);

        // Create and delete inactive form
        var inactiveForm = new
        {
            form = new
            {
                id = inactiveFormId,
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

        await _client.PostAsJsonAsync("/api/forms", inactiveForm);
        await _client.DeleteAsync($"/api/forms/{inactiveFormId}");

        // Act
        var response = await _client.GetAsync("/api/forms");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var forms = await response.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        forms.Should().NotBeNull();
        forms.Should().Contain(f => f.FormId == activeFormId);
        forms.Should().NotContain(f => f.FormId == inactiveFormId);
    }

    [Fact]
    public async Task GetForms_WithIncludeInactiveTrue_ShouldReturnAllForms()
    {
        // Arrange
        var activeFormId = "active-with-inactive-1";
        var inactiveFormId = "inactive-with-inactive-2";

        // Create active form
        var activeForm = new
        {
            form = new
            {
                id = activeFormId,
                title = "Active Form",
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

        await _client.PostAsJsonAsync("/api/forms", activeForm);

        // Create and delete inactive form
        var inactiveForm = new
        {
            form = new
            {
                id = inactiveFormId,
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

        await _client.PostAsJsonAsync("/api/forms", inactiveForm);
        await _client.DeleteAsync($"/api/forms/{inactiveFormId}");

        // Act
        var response = await _client.GetAsync("/api/forms?includeInactive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var forms = await response.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        forms.Should().NotBeNull();
        forms.Should().Contain(f => f.FormId == activeFormId && f.IsActive == true);
        forms.Should().Contain(f => f.FormId == inactiveFormId && f.IsActive == false);
    }

    [Fact]
    public async Task GetForms_ShouldReturn500_OnServerError()
    {
        // This test will verify error handling once implemented
        // For now, we're documenting the expected contract
        // In implementation, this would be tested with a fault injection or mock

        // Act & Assert
        // When server error occurs, should return 500 with Error schema:
        // {
        //   "message": "Internal server error",
        //   "details": "Error details"
        // }

        // This test is a placeholder and should be implemented with proper fault injection
        await Task.CompletedTask;
    }
}

/// <summary>
/// DTO matching FormSummary schema from OpenAPI spec
/// </summary>
public class FormSummaryDto
{
    public string FormId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
