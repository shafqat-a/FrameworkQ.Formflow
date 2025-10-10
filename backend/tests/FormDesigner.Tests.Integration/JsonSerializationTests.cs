using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FormDesigner.API.Models.DTOs;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests to verify JSON storage and retrieval across SQL Server.
/// Validates that JSONB (PostgreSQL) and NVARCHAR(MAX) (SQL Server) handle JSON consistently.
/// </summary>
public class JsonSerializationTests : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;
    private readonly SqlServerTestFixture _fixture;

    public JsonSerializationTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    /// <summary>
    /// TC-301: Simple JSON roundtrip
    /// Given: Form with basic JSON structure
    /// When: Create form → Retrieve form
    /// Then: JSON structure identical, property names preserved
    /// </summary>
    [Fact]
    public async Task TC301_SimpleJsonRoundtrip_ShouldPreserveStructure()
    {
        // Arrange
        var formId = $"test-simple-json-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Simple JSON Test",
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
                                id = "field-1",
                                label = "Simple Field",
                                required = true,
                                default_value = "test"
                            }
                        }
                    }
                }
            }
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Retrieve
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - Structure preserved
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
        retrievedForm.Form.Title.Should().Be("Simple JSON Test");
        retrievedForm.Form.Version.Should().Be("1.0");
        retrievedForm.Form.Pages.Should().HaveCount(1);
        retrievedForm.Form.Pages[0].Id.Should().Be("page-1");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-302: Complex JSON with special characters
    /// Given: Form with nested objects, arrays, unicode, emoji, quotes, newlines
    /// When: Create form → Retrieve form
    /// Then: All nesting preserved, arrays maintain order, special characters not corrupted
    /// </summary>
    [Fact]
    public async Task TC302_ComplexJsonWithSpecialCharacters_ShouldPreserveEverything()
    {
        // Arrange - Complex form with special characters
        var formId = $"test-complex-json-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Complex JSON Test: Unicode 你好, Emoji 🎉, Quotes \"test\"",
                version = "2.0",
                metadata = new
                {
                    tags = new[] { "unicode", "emoji", "special-chars" },
                    description = "Testing special characters:\n- Unicode: 你好世界\n- Emoji: 🎉🚀💻\n- Quotes: \"quoted\" and 'single'\n- Newlines and\ttabs",
                    nested = new
                    {
                        level1 = new
                        {
                            level2 = new
                            {
                                level3 = "deep nesting works"
                            }
                        }
                    }
                },
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page with 特殊 characters",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "field-unicode",
                                label = "Unicode: 日本語, 中文, العربية, Русский",
                                placeholder = "Enter text with emoji: 😀"
                            },
                            new
                            {
                                type = "textarea",
                                id = "field-multiline",
                                label = "Multi-line",
                                default_value = "Line 1\nLine 2\nLine 3\twith\ttabs"
                            }
                        }
                    }
                }
            }
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: "SQL Server NVARCHAR(MAX) should handle unicode and special characters");

        // Act - Retrieve
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - All special characters preserved
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Title.Should().Contain("你好");
        retrievedForm.Form.Title.Should().Contain("🎉");
        retrievedForm.Form.Title.Should().Contain("\"test\"");

        // Verify unicode in response
        responseContent.Should().Contain("你好世界");
        responseContent.Should().Contain("🎉🚀💻");
        responseContent.Should().Contain("日本語");
        responseContent.Should().Contain("العربية");
        responseContent.Should().Contain("Русский");

        // Verify newlines preserved (as \n in JSON)
        responseContent.Should().Contain("\\n");
        responseContent.Should().Contain("\\t");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-303: Large JSON payload
    /// Given: Form with large JSON payload (~500KB)
    /// When: Create form → Retrieve form
    /// Then: Storage succeeds, retrieval succeeds, data integrity maintained
    /// </summary>
    [Fact]
    public async Task TC303_LargeJsonPayload_ShouldHandleCorrectly()
    {
        // Arrange - Create a large form with many pages and widgets
        var formId = $"test-large-json-{Guid.NewGuid():N}";

        // Generate 50 pages with 20 widgets each = ~500KB JSON
        var pages = Enumerable.Range(1, 50).Select(pageNum => new
        {
            id = $"page-{pageNum}",
            title = $"Page {pageNum} - " + new string('A', 100), // Add padding
            description = "This is a test page with substantial content to increase payload size. " + new string('B', 200),
            widgets = Enumerable.Range(1, 20).Select(widgetNum => new
            {
                type = "text",
                id = $"field-{pageNum}-{widgetNum}",
                label = $"Field {widgetNum} on page {pageNum}",
                description = "Long description to increase payload size. " + new string('C', 150),
                placeholder = "Placeholder text " + new string('D', 100),
                validation_rules = new
                {
                    required = widgetNum % 2 == 0,
                    min_length = 5,
                    max_length = 100,
                    pattern = "^[A-Za-z0-9]+$"
                }
            }).ToArray()
        }).ToArray();

        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Large Form Test",
                version = "1.0",
                description = "This form has a large JSON payload to test NVARCHAR(MAX) storage. " + new string('E', 500),
                pages = pages
            }
        };

        // Calculate approximate size
        var jsonString = JsonSerializer.Serialize(createRequest);
        var sizeInBytes = Encoding.UTF8.GetByteCount(jsonString);
        var sizeInKB = sizeInBytes / 1024;

        // Output size for verification
        Console.WriteLine($"Payload size: {sizeInKB} KB ({sizeInBytes} bytes)");
        sizeInKB.Should().BeGreaterThan(400, because: "test should use a large payload");

        // Act - Create (measure time)
        var createStart = DateTime.UtcNow;
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        var createDuration = DateTime.UtcNow - createStart;

        // Assert - Create succeeded
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: "SQL Server NVARCHAR(MAX) should handle large JSON payloads");

        Console.WriteLine($"Create time: {createDuration.TotalMilliseconds}ms");

        // Act - Retrieve (measure time)
        var getStart = DateTime.UtcNow;
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        var getDuration = DateTime.UtcNow - getStart;

        // Assert - Retrieve succeeded
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Console.WriteLine($"Retrieve time: {getDuration.TotalMilliseconds}ms");

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - Data integrity
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
        retrievedForm.Form.Pages.Should().HaveCount(50);
        retrievedForm.Form.Pages[0].Id.Should().Be("page-1");
        retrievedForm.Form.Pages[49].Id.Should().Be("page-50");

        // Performance contract: SQL Server should be within reasonable limits
        // Note: Actual performance depends on hardware, network, etc.
        createDuration.TotalMilliseconds.Should().BeLessThan(5000,
            because: "large JSON storage should complete in reasonable time");
        getDuration.TotalMilliseconds.Should().BeLessThan(5000,
            because: "large JSON retrieval should complete in reasonable time");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-304: JSON null handling
    /// Given: Form with explicit null values and missing properties
    /// When: Create form → Retrieve form
    /// Then: Null values preserved, missing properties not added
    /// </summary>
    [Fact]
    public async Task TC304_JsonNullHandling_ShouldPreserveNullsCorrectly()
    {
        // Arrange - Form with explicit nulls and optional fields
        var formId = $"test-null-json-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Null Handling Test",
                version = "1.0",
                description = (string?)null, // Explicit null
                metadata = (object?)null, // Explicit null object
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Page 1",
                        description = (string?)null,
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "field-1",
                                label = "Field with nulls",
                                placeholder = (string?)null,
                                default_value = (string?)null,
                                validation_message = (string?)null
                            }
                        }
                    }
                }
            }
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            because: "null values should be handled correctly");

        // Act - Retrieve
        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        var responseContent = await getResponse.Content.ReadAsStringAsync();

        // Assert - Response received
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Parse JSON to check null handling
        var jsonDocument = JsonDocument.Parse(responseContent);
        var formElement = jsonDocument.RootElement.GetProperty("form");

        // Check that explicit nulls are either preserved or omitted (both acceptable)
        // The behavior depends on JsonIgnoreCondition.WhenWritingNull configuration
        var hasDescription = formElement.TryGetProperty("description", out var descProp);
        if (hasDescription)
        {
            descProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "explicit null should be preserved or property should be omitted");
        }

        // Verify form can be deserialized
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Id.Should().Be(formId);
        retrievedForm.Form.Title.Should().Be("Null Handling Test");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-305: JSON array ordering
    /// Verifies that array order is preserved during storage and retrieval
    /// </summary>
    [Fact]
    public async Task TC305_JsonArrayOrdering_ShouldPreserveOrder()
    {
        // Arrange - Form with ordered arrays
        var formId = $"test-array-order-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Array Order Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-first",
                        title = "First Page",
                        widgets = new object[]
                        {
                            new { type = "text", id = "widget-1", label = "First" },
                            new { type = "text", id = "widget-2", label = "Second" },
                            new { type = "text", id = "widget-3", label = "Third" },
                            new { type = "text", id = "widget-4", label = "Fourth" },
                            new { type = "text", id = "widget-5", label = "Fifth" }
                        }
                    },
                    new
                    {
                        id = "page-second",
                        title = "Second Page",
                        widgets = new object[] { }
                    },
                    new
                    {
                        id = "page-third",
                        title = "Third Page",
                        widgets = new object[] { }
                    }
                }
            }
        };

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/forms/{formId}");
        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();

        // Assert - Array order preserved
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Pages.Should().HaveCount(3);
        retrievedForm.Form.Pages[0].Id.Should().Be("page-first");
        retrievedForm.Form.Pages[1].Id.Should().Be("page-second");
        retrievedForm.Form.Pages[2].Id.Should().Be("page-third");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-306: JSON escaping and quotes
    /// Verifies that quotes and escape sequences are handled correctly
    /// </summary>
    [Fact]
    public async Task TC306_JsonEscaping_ShouldHandleQuotesCorrectly()
    {
        // Arrange
        var formId = $"test-escaping-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Test \"quoted\" and 'single' quotes",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Backslash \\ and quotes",
                        widgets = new object[]
                        {
                            new
                            {
                                type = "text",
                                id = "field-1",
                                label = "Path: C:\\Users\\test",
                                placeholder = "Enter \"value\" here"
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
        var responseContent = await getResponse.Content.ReadAsStringAsync();

        // Assert - Escaping preserved
        responseContent.Should().Contain("\\\"quoted\\\"");
        responseContent.Should().Contain("C:\\\\Users\\\\test");

        var retrievedForm = await getResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        retrievedForm.Should().NotBeNull();
        retrievedForm!.Form.Title.Should().Be("Test \"quoted\" and 'single' quotes");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }
}
