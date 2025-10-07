using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Apply EF Core migrations to create schema
        ApplyMigrations();
    }

    private void ApplyMigrations()
    {
        // Apply SQL Server setup script instead of migrations
        // (migrations were created for PostgreSQL and contain PostgreSQL-specific types)
        var setupScriptPath = "/Users/shafqat/git/FrameworkQ.Formflow/backend/database/setup.sqlserver.sql";

        if (!File.Exists(setupScriptPath))
        {
            Console.WriteLine($"ERROR: Setup script not found at: {setupScriptPath}");
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            throw new FileNotFoundException($"SQL Server setup script not found at: {setupScriptPath}");
        }

        var setupScript = File.ReadAllText(setupScriptPath);

        using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        connection.Open();

        // Read and execute setup script line by line
        // Split by GO and execute each batch
        var lines = setupScript.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var currentBatch = new System.Text.StringBuilder();
        int executedBatches = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check for GO statement
            if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batchSql = currentBatch.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(batchSql))
                {
                    // Skip CREATE DATABASE and USE statements
                    if (batchSql.Contains("CREATE DATABASE formflow", StringComparison.OrdinalIgnoreCase) ||
                        batchSql.Contains("USE formflow", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBatch.Clear();
                        continue;
                    }

                    try
                    {
                        using var command = connection.CreateCommand();
                        command.CommandText = batchSql;
                        command.ExecuteNonQuery();
                        executedBatches++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in batch {executedBatches + 1}: {ex.Message}");
                        throw;
                    }
                }
                currentBatch.Clear();
            }
            else
            {
                currentBatch.AppendLine(line);
            }
        }

        // Execute final batch if any
        var finalBatch = currentBatch.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalBatch))
        {
            using var command = connection.CreateCommand();
            command.CommandText = finalBatch;
            command.ExecuteNonQuery();
            executedBatches++;
        }

        Console.WriteLine($"Successfully executed {executedBatches} batches");
        connection.Close();
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
