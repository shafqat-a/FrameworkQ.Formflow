using FluentAssertions;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests for SqlServerTestFixture to verify it correctly sets up test environment.
/// </summary>
public class SqlServerTestFixtureTests : IClassFixture<SqlServerTestFixture>
{
    private readonly SqlServerTestFixture _fixture;

    public SqlServerTestFixtureTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Fixture_ShouldProvideHttpClient()
    {
        // Assert
        _fixture.Client.Should().NotBeNull("HttpClient should be provided by fixture");
    }

    [Fact]
    public void Fixture_ShouldGenerateUniqueDatabaseName()
    {
        // Assert
        _fixture.DatabaseName.Should().NotBeNullOrEmpty("Database name should be generated");
        _fixture.DatabaseName.Should().StartWith("formflow_test_", "Database name should follow naming convention");
        _fixture.DatabaseName.Length.Should().BeGreaterThan(20, "Database name should include timestamp and guid");
    }

    [Fact]
    public void Fixture_ShouldProvideConnectionString()
    {
        // Assert
        _fixture.ConnectionString.Should().NotBeNullOrEmpty("Connection string should be provided");
        _fixture.ConnectionString.Should().Contain("Server=", "Connection should have Server parameter");
        _fixture.ConnectionString.Should().Contain(_fixture.DatabaseName, "Connection string should reference test database");
    }

    [Fact]
    public async Task Fixture_ShouldAllowApiCalls()
    {
        // Act
        var response = await _fixture.Client.GetAsync("/api/forms");

        // Assert
        response.Should().NotBeNull("API should respond");
        // Note: We expect success or not found, but not connection errors
        var statusCode = (int)response.StatusCode;
        statusCode.Should().BeLessThan(500, "API should not return server errors for basic calls");
    }
}
