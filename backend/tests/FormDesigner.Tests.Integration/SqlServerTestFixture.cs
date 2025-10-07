using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Test fixture for SQL Server integration tests.
/// Manages database lifecycle and provides isolated test environment.
/// Each test run uses a unique database to prevent interference.
/// </summary>
public class SqlServerTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;
    private readonly string _connectionString;

    public HttpClient Client { get; }
    public string DatabaseName => _databaseName;
    public string ConnectionString => _connectionString;

    public SqlServerTestFixture()
    {
        // Generate unique database name for this test run
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        _databaseName = $"formflow_test_{timestamp}_{guid}";

        // Build connection string from environment or use default
        var server = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost,1433";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong!Passw0rd";

        _connectionString = $"Server={server};Database={_databaseName};User Id={userId};Password={password};TrustServerCertificate=True;";

        // Create custom WebApplicationFactory with SQL Server configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Override configuration to use SQL Server
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "SqlServer",
                        ["Database:ConnectionStringName"] = "FormDesignerDb",
                        ["ConnectionStrings:FormDesignerDb"] = _connectionString
                    });
                });
            });

        Client = _factory.CreateClient();

        // Create test database
        CreateTestDatabase();
    }

    private void CreateTestDatabase()
    {
        // Connection string to master database to create test database
        var server = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost,1433";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong!Passw0rd";

        var masterConnectionString = $"Server={server};Database=master;User Id={userId};Password={password};TrustServerCertificate=True;";

        using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
        connection.Open();

        // Create database if it doesn't exist
        using var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $@"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{_databaseName}')
            BEGIN
                CREATE DATABASE [{_databaseName}];
            END";
        createDbCommand.ExecuteNonQuery();

        connection.Close();

        // Wait a moment for database creation to complete
        System.Threading.Thread.Sleep(1000);

        // Run migrations on the new database
        // Note: In a real scenario, you'd use EF Core migrations here
        // For now, we'll assume migrations are applied automatically on first connection
    }

    public void Dispose()
    {
        try
        {
            // Clean up: Drop the test database
            DropTestDatabase();
        }
        catch (Exception ex)
        {
            // Log but don't fail if cleanup fails
            Console.WriteLine($"Warning: Failed to drop test database {_databaseName}: {ex.Message}");
        }
        finally
        {
            Client?.Dispose();
            _factory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    private void DropTestDatabase()
    {
        var server = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost,1433";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong!Passw0rd";

        var masterConnectionString = $"Server={server};Database=master;User Id={userId};Password={password};TrustServerCertificate=True;";

        using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
        connection.Open();

        // Set database to single user mode and drop
        using var dropDbCommand = connection.CreateCommand();
        dropDbCommand.CommandText = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{_databaseName}')
            BEGIN
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_databaseName}];
            END";
        dropDbCommand.ExecuteNonQuery();

        connection.Close();
    }
}
