using Microsoft.AspNetCore.Mvc.Testing;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Test fixture for integration tests
/// Provides configured HttpClient for testing end-to-end scenarios
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpClient Client { get; }

    public IntegrationTestFixture()
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
