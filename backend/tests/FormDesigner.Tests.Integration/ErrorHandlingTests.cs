using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormDesigner.API.Data;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests for database error scenarios.
/// Verifies that connection failures, missing databases, and schema issues are handled gracefully.
/// </summary>
public class ErrorHandlingTests
{
    /// <summary>
    /// TC-701: Connection failure handling
    /// Given: Invalid connection string
    /// When: Application starts
    /// Then: Clear error message logged, application fails to start
    /// </summary>
    [Fact]
    public void TC701_ConnectionFailure_ShouldFailGracefully()
    {
        // Arrange
        var invalidConnectionString = "Server=invalid-server-12345,1433;Database=test;User Id=sa;Password=wrong;Connection Timeout=1;";

        // Act & Assert
        var act = () =>
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Database:Provider"] = "SqlServer",
                            ["Database:ConnectionStringName"] = "FormDesignerDb",
                            ["ConnectionStrings:FormDesignerDb"] = invalidConnectionString
                        });
                    });
                });

            // Try to use DbContext - should fail
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.CanConnect(); // This should throw

            factory.Dispose();
        };

        // Assert - Connection should fail
        act.Should().Throw<Exception>(because: "invalid connection should cause error");
    }

    /// <summary>
    /// TC-702: Database does not exist
    /// Given: Connection string to non-existent database
    /// When: Attempting to connect
    /// Then: Clear error message about database not found
    /// </summary>
    [Fact]
    public void TC702_DatabaseDoesNotExist_ShouldIndicateMissingDatabase()
    {
        // Arrange
        var server = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost,1433";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong!Passw0rd";

        var nonExistentDbConnectionString = $"Server={server};Database=nonexistent_db_{Guid.NewGuid():N};User Id={userId};Password={password};TrustServerCertificate=True;Connection Timeout=5;";

        // Act
        var act = () =>
        {
            using var connection = new SqlConnection(nonExistentDbConnectionString);
            connection.Open(); // This should fail
        };

        // Assert
        act.Should().Throw<SqlException>()
            .WithMessage("*database*", because: "error should mention database issue");
    }

    /// <summary>
    /// TC-703: Schema validation - pending migrations detection
    /// Verifies that the application can detect when migrations need to be applied
    /// </summary>
    [Fact]
    public async Task TC703_PendingMigrations_ShouldBeDetectable()
    {
        // Arrange - Create a factory with a test database
        var server = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost,1433";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong!Passw0rd";
        var testDbName = $"formflow_migration_test_{Guid.NewGuid():N}";

        var connectionString = $"Server={server};Database={testDbName};User Id={userId};Password={password};TrustServerCertificate=True;";

        // Create empty database
        var masterConnectionString = $"Server={server};Database=master;User Id={userId};Password={password};TrustServerCertificate=True;";
        using (var connection = new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();
            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = $"CREATE DATABASE [{testDbName}]";
            await createCommand.ExecuteNonQueryAsync();
        }

        try
        {
            // Act - Check for pending migrations
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Database:Provider"] = "SqlServer",
                            ["Database:ConnectionStringName"] = "FormDesignerDb",
                            ["ConnectionStrings:FormDesignerDb"] = connectionString
                        });
                    });
                });

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            var hasPendingMigrations = pendingMigrations.Any();

            // Assert
            hasPendingMigrations.Should().BeTrue(
                because: "empty database should have pending migrations");

            // Apply migrations
            await dbContext.Database.MigrateAsync();

            // Verify no more pending migrations
            var pendingAfter = await dbContext.Database.GetPendingMigrationsAsync();
            pendingAfter.Should().BeEmpty(because: "all migrations should be applied");

            factory.Dispose();
        }
        finally
        {
            // Cleanup - Drop test database
            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = $@"
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{testDbName}')
                BEGIN
                    ALTER DATABASE [{testDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{testDbName}];
                END";
            await dropCommand.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// TC-704: Invalid provider configuration
    /// Tests error handling for unsupported database providers
    /// </summary>
    [Fact]
    public void TC704_InvalidProvider_ShouldThrowException()
    {
        // Act & Assert
        var act = () =>
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Database:Provider"] = "Oracle", // Unsupported provider
                            ["Database:ConnectionStringName"] = "FormDesignerDb",
                            ["ConnectionStrings:FormDesignerDb"] = "Data Source=localhost;User Id=test;Password=test;"
                        });
                    });
                });

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            factory.Dispose();
        };

        // Assert - Should fail with unsupported provider
        act.Should().Throw<Exception>(because: "unsupported provider should cause error");
    }

    /// <summary>
    /// TC-705: Missing connection string
    /// Tests error handling when connection string is missing
    /// </summary>
    [Fact]
    public void TC705_MissingConnectionString_ShouldThrowException()
    {
        // Act & Assert
        var act = () =>
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Database:Provider"] = "SqlServer"
                            // Missing ConnectionStrings:FormDesignerDb
                        });
                    });
                });

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.CanConnect();

            factory.Dispose();
        };

        // Assert
        act.Should().Throw<Exception>(because: "missing connection string should cause error");
    }
}
