using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Contract tests for PUT /api/forms/{id} endpoint
/// Validates compliance with OpenAPI specification: forms-api.yaml
/// </summary>
public class FormsApiPutUpdateTests : IClassFixture<ContractTestFixture>
{
    private readonly HttpClient _client;

    public FormsApiPutUpdateTests(ContractTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task PutForm_ShouldReturn200_WithUpdatedForm_WhenValidUpdateProvided()
    {
        // Arrange
        var formId = "update-test-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Original Title",
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

        var updateRequest = new
        {
            form = new
            {
                id = formId,
                title = "Updated Title",
                version = "2.0",
                pages = new[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Updated Page 1",
                        sections = new[]
                        {
                            new
                            {
                                id = "section-1",
                                title = "Updated Section 1",
                                widgets = new[]
                                {
                                    new
                                    {
                                        type = "field",
                                        id = "new-field",
                                        title = "New Field",
                                        field = new
                                        {
                                            name = "new_field",
                                            label = "New",
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

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var updatedForm = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        updatedForm.Should().NotBeNull();
        updatedForm!.Form.Id.Should().Be(formId);
        updatedForm.Form.Title.Should().Be("Updated Title");
        updatedForm.Form.Version.Should().Be("2.0");
    }

    [Fact]
    public async Task PutForm_ShouldReturn404_WhenFormDoesNotExist()
    {
        // Arrange
        var nonExistentFormId = "non-existent-update-form";
        var updateRequest = new
        {
            form = new
            {
                id = nonExistentFormId,
                title = "This Form Does Not Exist",
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
        var response = await _client.PutAsJsonAsync($"/api/forms/{nonExistentFormId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PutForm_ShouldReturn400_WhenIdInUrlAndBodyMismatch()
    {
        // Arrange
        var formId = "mismatch-test-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
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
                                widgets = new object[] { }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        var updateRequest = new
        {
            form = new
            {
                id = "different-id",  // Mismatch with URL
                title = "Updated Title",
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
        var response = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("mismatch").Or.Contain("does not match");
    }

    [Fact]
    public async Task PutForm_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var formId = "validation-test-form";
        var createRequest = new
        {
            form = new
            {
                id = formId,
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
                                widgets = new object[] { }
                            }
                        }
                    }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        var invalidUpdateRequest = new
        {
            form = new
            {
                id = formId,
                // Missing required title field
                version = "2.0",
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
        var response = await _client.PutAsJsonAsync($"/api/forms/{formId}", invalidUpdateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().ContainKey("form.title");
    }

    [Fact]
    public async Task PutForm_ShouldPerformFullReplacement_NotPartialUpdate()
    {
        // Arrange
        var formId = "full-replacement-test";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Original Form",
                version = "1.0",
                meta = new
                {
                    organization = "Original Org",
                    document_no = "DOC-001"
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

        // Update without meta - should remove meta completely
        var updateRequest = new
        {
            form = new
            {
                id = formId,
                title = "Replaced Form",
                version = "2.0",
                // No meta field - should be null/absent after update
                pages = new[]
                {
                    new
                    {
                        id = "page-2",
                        title = "Page 2",
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

        // Act
        var response = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedForm = await response.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        updatedForm.Should().NotBeNull();
        updatedForm!.Form.Meta.Should().BeNull();  // Meta should be removed
        updatedForm.Form.Pages.Should().HaveCount(1);
        updatedForm.Form.Pages[0].Id.Should().Be("page-2");  // Pages completely replaced
    }

    [Theory]
    [InlineData("invalid form id")]
    [InlineData("UPPERCASE")]
    [InlineData("form@123")]
    public async Task PutForm_ShouldReturn400_WhenIdPatternIsInvalid(string invalidId)
    {
        // Arrange
        var updateRequest = new
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
        var response = await _client.PutAsJsonAsync($"/api/forms/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
