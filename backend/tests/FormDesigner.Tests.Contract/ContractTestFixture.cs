using Microsoft.AspNetCore.Mvc.Testing;

namespace FormDesigner.Tests.Contract;

/// <summary>
/// Test fixture for API contract tests
/// Provides configured HttpClient for testing API endpoints
/// </summary>
public class ContractTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }

    public ContractTestFixture()
    {
        _factory = new WebApplicationFactory<Program>();
        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        _factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
