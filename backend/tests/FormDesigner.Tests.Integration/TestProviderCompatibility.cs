using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using FormDesigner.API.Models.DTOs;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// TC-601-TC-604: Verify existing tests work on SQL Server.
/// This test class demonstrates that CRUD operations work identically on SQL Server.
/// </summary>
public class TestProviderCompatibility : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;

    public TestProviderCompatibility(SqlServerTestFixture fixture)
    {
        _client = fixture.Client;
    }

    /// <summary>
    /// TC-601: Verify basic CRUD operations work on SQL Server
    /// This is a smoke test that proves the existing test suite would pass on SQL Server
    /// </summary>
    [Fact]
    public async Task TC601_BasicCrudOperations_ShouldWorkOnSqlServer()
    {
        // Arrange - Create a simple form
        var formId = $"test-crud-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "SQL Server Compatibility Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Test Page",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "test-field",
                                label = "Test Field"
                            }
                        }
                    }
                }
            }
        };

        // Act - CREATE
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Assert - CREATE
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: "form creation should work on SQL Server");

        // Act - READ
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");

        // Assert - READ
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "form retrieval should work on SQL Server");

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
        retrievedForm.Form.Title.Should().Be("SQL Server Compatibility Test");

        // Act - UPDATE
        var updateRequest = new
        {
            form = new
            {
                id = formId,
                title = "Updated Title",
                version = "1.1",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Updated Page",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "test-field",
                                label = "Updated Field"
                            }
                        }
                    }
                }
            }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);

        // Assert - UPDATE
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "form update should work on SQL Server");

        // Verify update persisted
        var getAfterUpdate = await _client.GetAsync($"/api/forms/{formId}");
        var updatedForm = await getAfterUpdate.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        updatedForm!.Form.Title.Should().Be("Updated Title");

        // Act - DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/forms/{formId}");

        // Assert - DELETE
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            because: "form deletion should work on SQL Server");

        // Verify deletion (form should not be in active list)
        var getAfterDelete = await _client.GetAsync($"/api/forms/{formId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound,
            because: "deleted form should not be retrievable");
    }

    /// <summary>
    /// TC-602: Verify form listing works on SQL Server
    /// </summary>
    [Fact]
    public async Task TC602_ListForms_ShouldWorkOnSqlServer()
    {
        // Arrange - Create multiple forms
        var form1Id = $"test-list-1-{Guid.NewGuid():N}";
        var form2Id = $"test-list-2-{Guid.NewGuid():N}";

        var form1 = new
        {
            form = new
            {
                id = form1Id,
                title = "Form 1",
                version = "1.0",
                pages = new object[] { new { id = "p1", title = "Page 1", widgets = new object[] { } } }
            }
        };

        var form2 = new
        {
            form = new
            {
                id = form2Id,
                title = "Form 2",
                version = "1.0",
                pages = new object[] { new { id = "p1", title = "Page 1", widgets = new object[] { } } }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", form1);
        await _client.PostAsJsonAsync("/api/forms", form2);

        // Act - List all forms
        var listResponse = await _client.GetAsync("/api/forms");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "listing forms should work on SQL Server");

        var forms = await listResponse.Content.ReadFromJsonAsync<List<FormDefinitionRootDto>>();
        forms.Should().NotBeNull();
        forms.Should().Contain(f => f.Form.Id == form1Id);
        forms.Should().Contain(f => f.Form.Id == form2Id);

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{form1Id}");
        await _client.DeleteAsync($"/api/forms/{form2Id}");
    }

    /// <summary>
    /// TC-603: Verify complex form structures work on SQL Server
    /// </summary>
    [Fact]
    public async Task TC603_ComplexFormStructure_ShouldWorkOnSqlServer()
    {
        // Arrange - Create form with nested structure
        var formId = $"test-complex-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Complex Form",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Personal Info",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "first-name",
                                label = "First Name",
                                required = true
                            },
                            new
                            {
                                type = "text",
                                id = "last-name",
                                label = "Last Name",
                                required = true
                            }
                        }
                    },
                    new
                    {
                        id = "page-2",
                        title = "Address",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "street",
                                label = "Street"
                            },
                            new
                            {
                                type = "text",
                                id = "city",
                                label = "City"
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
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - Verify structure integrity
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Pages.Should().HaveCount(2);
        retrievedForm.Form.Pages[0].Id.Should().Be("page-1");
        retrievedForm.Form.Pages[1].Id.Should().Be("page-2");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-604: Verify validation errors work correctly on SQL Server
    /// </summary>
    [Fact]
    public async Task TC604_ValidationErrors_ShouldWorkOnSqlServer()
    {
        // Arrange - Create invalid form (missing required fields)
        var invalidRequest = new
        {
            form = new
            {
                // Missing 'id' field
                title = "Invalid Form",
                version = "1.0"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/forms", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "validation should work the same on SQL Server");

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        error.Should().NotBeNull();
        error!.Errors.Should().NotBeEmpty(because: "validation errors should be returned");
    }

    /// <summary>
    /// TC-605: Verify concurrent operations work on SQL Server
    /// </summary>
    [Fact]
    public async Task TC605_ConcurrentOperations_ShouldWorkOnSqlServer()
    {
        // Arrange - Create multiple forms concurrently
        var formIds = Enumerable.Range(1, 5)
            .Select(_ => $"test-concurrent-{Guid.NewGuid():N}")
            .ToList();

        var createTasks = formIds.Select(async formId =>
        {
            var request = new
            {
                form = new
                {
                    id = formId,
                    title = $"Concurrent Test {formId}",
                    version = "1.0",
                    pages = new object[] { new { id = "p1", title = "Page", widgets = new object[] { } } }
                }
            };
            return await _client.PostAsJsonAsync("/api/forms", request);
        });

        // Act - Execute concurrently
        var responses = await Task.WhenAll(createTasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created),
            because: "concurrent operations should work on SQL Server");

        // Verify all were created
        foreach (var formId in formIds)
        {
            var getResponse = await _client.GetAsync($"/api/forms/{formId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Cleanup
        var deleteTasks = formIds.Select(formId => _client.DeleteAsync($"/api/forms/{formId}"));
        await Task.WhenAll(deleteTasks);
    }
}
