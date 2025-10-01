using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for DELETE /api/forms/{id} endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class FormsApiDeleteTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public FormsApiDeleteTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task DeleteForm_ShouldReturn204_WhenFormExists()
    {
        // Arrange
        var formId = "delete-test-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Form to Delete",
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
        var response = await _client.DeleteAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Headers.ContentLength.Should().Be(0);
    }

    [Fact]
    public async Task DeleteForm_ShouldPerformSoftDelete_NotHardDelete()
    {
        // Arrange
        var formId = "soft-delete-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Soft Delete Test",
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
        var deleteResponse = await _client.DeleteAsync($"/api/forms/{formId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify form is not in active list
        var listResponse = await _client.GetAsync("/api/forms");
        var activeForms = await listResponse.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        activeForms.Should().NotContain(f => f.FormId == formId);

        // Verify form still exists when includeInactive=true
        var listWithInactiveResponse = await _client.GetAsync("/api/forms?includeInactive=true");
        var allForms = await listWithInactiveResponse.Content.ReadFromJsonAsync<List<FormSummaryDto>>();
        allForms.Should().Contain(f => f.FormId == formId && f.IsActive == false);
    }

    [Fact]
    public async Task DeleteForm_ShouldReturn404_WhenFormDoesNotExist()
    {
        // Arrange
        var nonExistentFormId = "non-existent-delete-form";

        // Act
        var response = await _client.DeleteAsync($"/api/forms/{nonExistentFormId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Message.Should().Contain("not found").Or.Contain("does not exist");
    }

    [Fact]
    public async Task DeleteForm_ShouldReturn404_WhenFormAlreadyDeleted()
    {
        // Arrange
        var formId = "already-deleted-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Already Deleted Form",
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
        await _client.DeleteAsync($"/api/forms/{formId}");  // First delete

        // Act - Try to delete again
        var response = await _client.DeleteAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("invalid form id")]
    [InlineData("UPPERCASE-FORM")]
    [InlineData("form@special")]
    public async Task DeleteForm_ShouldReturn400_WhenIdPatternIsInvalid(string invalidId)
    {
        // Arrange & Act
        var response = await _client.DeleteAsync($"/api/forms/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("pattern").Or.Contain("invalid");
    }

    [Fact]
    public async Task DeleteForm_ShouldNotAffectGetById_BeforeDelete()
    {
        // Arrange
        var formId = "get-after-delete-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Get After Delete Test",
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

        // Verify form is accessible before delete
        var getBeforeResponse = await _client.GetAsync($"/api/forms/{formId}");
        getBeforeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete form
        var deleteResponse = await _client.DeleteAsync($"/api/forms/{formId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Form should not be accessible after delete
        var getAfterResponse = await _client.GetAsync($"/api/forms/{formId}");
        getAfterResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
