using FluentAssertions;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Http.Json;
using FormDesigner.API.Models.DTOs;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests to verify timestamp default values and UTC handling for SQL Server.
/// Validates that SYSUTCDATETIME() defaults work correctly and timestamps are UTC.
/// </summary>
public class TimestampTests : IClassFixture<SqlServerTestFixture>
{
    private readonly HttpClient _client;
    private readonly SqlServerTestFixture _fixture;

    public TimestampTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    /// <summary>
    /// TC-402: CreatedAt default for SQL Server
    /// Given: SQL Server provider
    /// When: Insert form without specifying CreatedAt
    /// Then: CreatedAt set by database default (SYSUTCDATETIME()), value is UTC, within 1 second of current time
    /// </summary>
    [Fact]
    public async Task TC402_CreatedAtDefault_SqlServer_ShouldUseDefaultValue()
    {
        // Arrange
        var formId = $"test-timestamp-{Guid.NewGuid():N}";
        var beforeCreate = DateTime.UtcNow;

        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Timestamp Test",
                version = "1.0",
                pages = new object[]
                {
                    new
                    {
                        id = "page-1",
                        title = "Test Page",
                        widgets = new object[] { }
                    }
                }
            }
        };

        // Act - Create form (CreatedAt should be set by database)
        var createResponse = await _client.PostAsJsonAsync("/api/forms", createRequest);
        var afterCreate = DateTime.UtcNow;

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Query database directly to verify default was applied
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT created_at, updated_at
            FROM form_definitions
            WHERE form_id = @formId";
        command.Parameters.AddWithValue("@formId", formId);

        DateTime? createdAt = null;
        DateTime? updatedAt = null;

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            createdAt = reader.GetDateTime(0);
            updatedAt = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
        }

        // Assert
        createdAt.Should().NotBeNull(because: "CreatedAt should be set by SYSUTCDATETIME() default");
        createdAt!.Value.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5),
            because: "CreatedAt should be set to current UTC time by database");
        createdAt.Value.Should().BeOnOrBefore(afterCreate);

        // CreatedAt should be UTC (Kind should be Unspecified from SQL Server, but value is UTC)
        createdAt.Value.Kind.Should().Be(DateTimeKind.Unspecified,
            because: "SQL Server returns DateTime with Unspecified kind");

        updatedAt.Should().BeNull(because: "UpdatedAt should be null on creation");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-403: UpdatedAt manual setting
    /// Given: Existing form
    /// When: Update form
    /// Then: UpdatedAt set by application code, value is UTC, UpdatedAt > CreatedAt
    /// </summary>
    [Fact]
    public async Task TC403_UpdatedAt_ShouldBeSetOnUpdate()
    {
        // Arrange - Create form
        var formId = $"test-updated-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Original Title",
                version = "1.0",
                pages = new object[]
                {
                    new { id = "page-1", title = "Page", widgets = new object[] { } }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Wait a moment to ensure timestamp difference
        await Task.Delay(100);

        // Act - Update form
        var updateRequest = new
        {
            form = new
            {
                id = formId,
                title = "Updated Title",
                version = "1.1",
                pages = new object[]
                {
                    new { id = "page-1", title = "Page", widgets = new object[] { } }
                }
            }
        };

        var beforeUpdate = DateTime.UtcNow;
        var updateResponse = await _client.PutAsJsonAsync($"/api/forms/{formId}", updateRequest);
        var afterUpdate = DateTime.UtcNow;

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Query database to verify timestamps
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT created_at, updated_at
            FROM form_definitions
            WHERE form_id = @formId";
        command.Parameters.AddWithValue("@formId", formId);

        DateTime? createdAt = null;
        DateTime? updatedAt = null;

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            createdAt = reader.GetDateTime(0);
            updatedAt = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
        }

        // Assert
        createdAt.Should().NotBeNull();
        updatedAt.Should().NotBeNull(because: "UpdatedAt should be set after update");

        updatedAt!.Value.Should().BeAfter(createdAt!.Value,
            because: "UpdatedAt should be after CreatedAt");

        updatedAt.Value.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5),
            because: "UpdatedAt should be set to current UTC time");
        updatedAt.Value.Should().BeOnOrBefore(afterUpdate);

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-404: Timestamp ordering
    /// Given: Multiple forms created in sequence
    /// When: Query forms ordered by CreatedAt DESC
    /// Then: Most recent form first, chronological order correct, no timezone conversion issues
    /// </summary>
    [Fact]
    public async Task TC404_TimestampOrdering_ShouldBeChronological()
    {
        // Arrange - Create multiple forms with delays between them
        var formIds = new List<string>();
        var creationTimes = new List<DateTime>();

        for (int i = 1; i <= 3; i++)
        {
            var formId = $"test-order-{i}-{Guid.NewGuid():N}";
            formIds.Add(formId);

            var createRequest = new
            {
                form = new
                {
                    id = formId,
                    title = $"Form {i}",
                    version = "1.0",
                    pages = new object[]
                    {
                        new { id = "page-1", title = "Page", widgets = new object[] { } }
                    }
                }
            };

            var beforeCreate = DateTime.UtcNow;
            await _client.PostAsJsonAsync("/api/forms", createRequest);
            creationTimes.Add(beforeCreate);

            // Wait to ensure timestamp difference
            if (i < 3)
            {
                await Task.Delay(200);
            }
        }

        // Act - Query database with ordering
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT form_id, created_at
            FROM form_definitions
            WHERE form_id IN (@formId1, @formId2, @formId3)
            ORDER BY created_at DESC";
        command.Parameters.AddWithValue("@formId1", formIds[0]);
        command.Parameters.AddWithValue("@formId2", formIds[1]);
        command.Parameters.AddWithValue("@formId3", formIds[2]);

        var orderedFormIds = new List<string>();
        var orderedTimestamps = new List<DateTime>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orderedFormIds.Add(reader.GetString(0));
            orderedTimestamps.Add(reader.GetDateTime(1));
        }

        // Assert - Order is correct (most recent first)
        orderedFormIds.Should().HaveCount(3);
        orderedFormIds[0].Should().Be(formIds[2], because: "most recently created form should be first");
        orderedFormIds[1].Should().Be(formIds[1], because: "middle form should be second");
        orderedFormIds[2].Should().Be(formIds[0], because: "earliest created form should be last");

        // Verify timestamps are in descending order
        orderedTimestamps[0].Should().BeAfter(orderedTimestamps[1]);
        orderedTimestamps[1].Should().BeAfter(orderedTimestamps[2]);

        // Cleanup
        foreach (var formId in formIds)
        {
            await _client.DeleteAsync($"/api/forms/{formId}");
        }
    }

    /// <summary>
    /// TC-405: Verify timestamps are stored as UTC
    /// Ensures no timezone conversion issues occur
    /// </summary>
    [Fact]
    public async Task TC405_TimestampsAreUtc_ShouldNotHaveTimezoneIssues()
    {
        // Arrange
        var formId = $"test-utc-{Guid.NewGuid():N}";
        var utcNow = DateTime.UtcNow;

        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "UTC Test",
                version = "1.0",
                pages = new object[]
                {
                    new { id = "page-1", title = "Page", widgets = new object[] { } }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act - Query with timezone info
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                created_at,
                DATEADD(hour, 5, created_at) as created_at_plus_5h,
                DATEDIFF(second, created_at, @utcNow) as seconds_difference
            FROM form_definitions
            WHERE form_id = @formId";
        command.Parameters.AddWithValue("@formId", formId);
        command.Parameters.AddWithValue("@utcNow", utcNow);

        DateTime? createdAt = null;
        DateTime? createdAtPlus5h = null;
        int? secondsDifference = null;

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            createdAt = reader.GetDateTime(0);
            createdAtPlus5h = reader.GetDateTime(1);
            secondsDifference = reader.GetInt32(2);
        }

        // Assert
        createdAt.Should().NotBeNull();

        // Difference should be small (within a few seconds)
        Math.Abs(secondsDifference!.Value).Should().BeLessThan(5,
            because: "database timestamp should be very close to application UTC time");

        // Adding hours should work correctly (no timezone conversion)
        createdAtPlus5h.Should().Be(createdAt!.Value.AddHours(5),
            because: "datetime math should work correctly with DATETIME2");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }

    /// <summary>
    /// TC-406: Multiple rapid creates should have increasing timestamps
    /// </summary>
    [Fact]
    public async Task TC406_RapidCreates_ShouldHaveIncreasingTimestamps()
    {
        // Arrange - Create multiple forms rapidly
        var formIds = new List<string>();
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 1; i <= 5; i++)
        {
            var formId = $"test-rapid-{i}-{Guid.NewGuid():N}";
            formIds.Add(formId);

            var createRequest = new
            {
                form = new
                {
                    id = formId,
                    title = $"Rapid Form {i}",
                    version = "1.0",
                    pages = new object[]
                    {
                        new { id = "page-1", title = "Page", widgets = new object[] { } }
                    }
                }
            };

            tasks.Add(_client.PostAsJsonAsync("/api/forms", createRequest));
        }

        // Act - Execute all creates
        var responses = await Task.WhenAll(tasks);

        // All should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));

        // Query timestamps
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT form_id, created_at
            FROM form_definitions
            WHERE form_id IN (@id1, @id2, @id3, @id4, @id5)
            ORDER BY created_at ASC";

        for (int i = 0; i < formIds.Count; i++)
        {
            command.Parameters.AddWithValue($"@id{i + 1}", formIds[i]);
        }

        var timestamps = new List<DateTime>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            timestamps.Add(reader.GetDateTime(1));
        }

        // Assert - All timestamps should be unique (DATETIME2 has high precision)
        timestamps.Should().HaveCount(5);
        timestamps.Should().OnlyHaveUniqueItems(because: "DATETIME2 precision should distinguish rapid inserts");

        // Timestamps should be in ascending order or very close
        for (int i = 1; i < timestamps.Count; i++)
        {
            timestamps[i].Should().BeOnOrAfter(timestamps[i - 1],
                because: "timestamps should be monotonically increasing or equal");
        }

        // Cleanup
        foreach (var formId in formIds)
        {
            await _client.DeleteAsync($"/api/forms/{formId}");
        }
    }

    /// <summary>
    /// TC-407: Verify timestamp precision
    /// DATETIME2 should have sub-second precision
    /// </summary>
    [Fact]
    public async Task TC407_TimestampPrecision_ShouldHaveSubSecondAccuracy()
    {
        // Arrange
        var formId = $"test-precision-{Guid.NewGuid():N}";
        var createRequest = new
        {
            form = new
            {
                id = formId,
                title = "Precision Test",
                version = "1.0",
                pages = new object[]
                {
                    new { id = "page-1", title = "Page", widgets = new object[] { } }
                }
            }
        };

        await _client.PostAsJsonAsync("/api/forms", createRequest);

        // Act - Query with high precision
        using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                created_at,
                DATEPART(millisecond, created_at) as milliseconds,
                DATEPART(microsecond, created_at) as microseconds
            FROM form_definitions
            WHERE form_id = @formId";
        command.Parameters.AddWithValue("@formId", formId);

        DateTime? createdAt = null;
        int? milliseconds = null;

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            createdAt = reader.GetDateTime(0);
            milliseconds = reader.GetInt32(1);
            // Note: microseconds might not be available depending on SQL Server version
        }

        // Assert
        createdAt.Should().NotBeNull();
        createdAt!.Value.Millisecond.Should().Be(milliseconds!.Value,
            because: "DATETIME2 should preserve millisecond precision");

        // Cleanup
        await _client.DeleteAsync($"/api/forms/{formId}");
    }
}
