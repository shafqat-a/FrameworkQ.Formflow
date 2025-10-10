using FluentAssertions;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FormDesigner.API.Models.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Performance benchmark tests for SQL Server.
/// Verifies that SQL Server meets performance criteria (SC-004, SC-005).
/// </summary>
public class PerformanceTests : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;
    private readonly SqlServerTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public PerformanceTests(SqlServerTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _client = fixture.Client;
        _output = output;
    }

    /// <summary>
    /// TC-802: Measure SQL Server startup time
    /// Given: SQL Server provider
    /// When: Measure application startup time
    /// Then: Record startup time for comparison
    /// </summary>
    [Fact]
    public async Task TC802_StartupTime_SqlServer_ShouldBeReasonable()
    {
        // Arrange - Application is already started by fixture
        var stopwatch = Stopwatch.StartNew();

        // Act - Make a simple request to ensure application is fully initialized
        var response = await _client.GetAsync("/api/forms");

        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "application should be running");

        var startupTime = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"SQL Server startup + first request time: {startupTime}ms");

        // Startup should be reasonable (< 10 seconds for first request)
        startupTime.Should().BeLessThan(10000,
            because: "application startup should complete in reasonable time");
    }

    /// <summary>
    /// TC-804: Measure SQL Server API response time
    /// Given: SQL Server provider, multiple forms in database
    /// When: GET /api/forms
    /// Then: Response time should be reasonable
    /// </summary>
    [Fact]
    public async Task TC804_ApiResponseTime_SqlServer_ShouldBeReasonable()
    {
        // Arrange - Create test data (10 forms)
        var formIds = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            var formId = $"perf-test-{i}-{Guid.NewGuid():N}";
            formIds.Add(formId);

            var createRequest = new
            {
                form = new
                {
                    id = formId,
                    title = $"Performance Test Form {i}",
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
                                    id = $"field-{i}",
                                    label = $"Field {i}"
                                }
                            }
                        }
                    }
                }
            };

            await _client.PostAsJsonAsync("/api/forms", createRequest);
        }

        // Warm up - First request may include JIT compilation
        await _client.GetAsync("/api/forms");

        // Act - Measure response time for list operation
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/forms");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseTime = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"SQL Server API response time (10 forms): {responseTime}ms");

        // Response time should be reasonable (< 1 second for 10 forms)
        responseTime.Should().BeLessThan(1000,
            because: "listing 10 forms should be fast");

        // Cleanup
        foreach (var formId in formIds)
        {
            await _client.DeleteAsync($"/api/forms/{formId}");
        }
    }

    /// <summary>
    /// TC-805: Measure individual CRUD operation performance
    /// </summary>
    [Fact]
    public async Task TC805_CrudOperationPerformance_ShouldBeReasonable()
    {
        // Arrange
        var formId = $"perf-crud-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "CRUD Performance Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Test Page",
                        widgets = new object[]
                        {
                            new { type = "text", id = "field-1", label = "Field 1" }
                        }
                    }
                }
            }
        };

        // Act & Measure - CREATE
        var createStopwatch = Stopwatch.StartNew();
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createStopwatch.Stop();

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _output.WriteLine($"CREATE time: {createStopwatch.ElapsedMilliseconds}ms");

        // Act & Measure - READ
        var readStopwatch = Stopwatch.StartNew();
        var readResponse = await _client.GetAsync($"/api/forms/{formId}");
        readStopwatch.Stop();

        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine($"READ time: {readStopwatch.ElapsedMilliseconds}ms");

        // Act & Measure - UPDATE
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
                            new { type = "text", id = "field-1", label = "Updated Field" }
                        }
                    }
                }
            }
        };

        var updateStopwatch = Stopwatch.StartNew();
        var updateResponse = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);
        updateStopwatch.Stop();

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine($"UPDATE time: {updateStopwatch.ElapsedMilliseconds}ms");

        // Act & Measure - DELETE
        var deleteStopwatch = Stopwatch.StartNew();
        var deleteResponse = await _client.DeleteAsync($"/api/forms/{formId}");
        deleteStopwatch.Stop();

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _output.WriteLine($"DELETE time: {deleteStopwatch.ElapsedMilliseconds}ms");

        // Assert - All operations should be fast
        createStopwatch.ElapsedMilliseconds.Should().BeLessThan(500, because: "CREATE should be fast");
        readStopwatch.ElapsedMilliseconds.Should().BeLessThan(200, because: "READ should be fast");
        updateStopwatch.ElapsedMilliseconds.Should().BeLessThan(500, because: "UPDATE should be fast");
        deleteStopwatch.ElapsedMilliseconds.Should().BeLessThan(200, because: "DELETE should be fast");
    }

    /// <summary>
    /// TC-806: Large JSON payload performance
    /// Measures performance with realistic large payloads
    /// </summary>
    [Fact]
    public async Task TC806_LargeJsonPerformance_ShouldBeAcceptable()
    {
        // Arrange - Large form (same as JSON tests)
        var formId = $"perf-large-{Guid.NewGuid():N}";

        var pages = Enumerable.Range(1, 20).Select(pageNum => new
        {
            id = $"page-{pageNum}",
            title = $"Page {pageNum}",
            widgets = Enumerable.Range(1, 10).Select(widgetNum => new
            {
                type = "text",
                id = $"field-{pageNum}-{widgetNum}",
                label = $"Field {widgetNum}",
                description = "Some description text " + new string('x', 100)
            }).ToArray()
        }).ToArray();

        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Large Form Performance Test",
                version = "1.0",
                pages = pages
            }
        };

        // Act & Measure - CREATE large form
        var createStopwatch = Stopwatch.StartNew();
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        createStopwatch.Stop();

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _output.WriteLine($"Large form CREATE time: {createStopwatch.ElapsedMilliseconds}ms");

        // Act & Measure - READ large form
        var readStopwatch = Stopwatch.StartNew();
        var readResponse = await _client.GetAsync($"/api/forms/{formId}");
        readStopwatch.Stop();

        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine($"Large form READ time: {readStopwatch.ElapsedMilliseconds}ms");

        var form = await readResponse.Content.ReadFromJsonAsync<FormDefinitionRootDto>();
        form!.Form.Pages.Should().HaveCount(20);

        // Assert - Performance acceptable for large payloads
        createStopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
            because: "large form creation should complete within 2 seconds");
        readStopwatch.ElapsedMilliseconds.Should().BeLessThan(1000,
            because: "large form retrieval should complete within 1 second");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }
}
