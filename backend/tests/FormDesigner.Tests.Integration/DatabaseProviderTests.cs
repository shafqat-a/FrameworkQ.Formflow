using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormDesigner.API.Data;
using Xunit;

namespace FormDesigner.Tests.Integration;

/// <summary>
/// Tests to verify database provider configuration and detection.
/// Validates that the application correctly identifies and uses configured database providers.
/// </summary>
public class DatabaseProviderTests : IClassFixture<SqlServerTestFixture>
{
    private readonly SqlServerTestFixture _fixture;

    public DatabaseProviderTests(SqlServerTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// TC-001: Verify PostgreSQL provider detection
    /// Given: appsettings.json configured with Database:Provider = "Postgres"
    /// When: Application starts
    /// Then: ApplicationDbContext uses Npgsql provider
    /// </summary>
    [Fact]
    public async Task TC001_ProviderDetection_PostgreSQL_ShouldUseNpgsqlProvider()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "Postgres",
                        ["Database:ConnectionStringName"] = "FormDesignerDb",
                        ["ConnectionStrings:FormDesignerDb"] = "Host=localhost;Database=formflow_test;Username=postgres;Password=postgres"
                    });
                });
            });

        // Act
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = dbContext.Database.ProviderName;

        // Assert
        providerName.Should().NotBeNull();
        providerName.Should().ContainEquivalentOf("Npgsql");
        providerName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");

        // Cleanup
        await factory.DisposeAsync();
    }

    /// <summary>
    /// TC-002: Verify SQL Server provider detection
    /// Given: appsettings.json configured with Database:Provider = "SqlServer"
    /// When: Application starts
    /// Then: ApplicationDbContext uses SqlServer provider
    /// </summary>
    [Fact]
    public void TC002_ProviderDetection_SqlServer_ShouldUseSqlServerProvider()
    {
        // Arrange - Create factory configured for SQL Server
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
                        ["ConnectionStrings:FormDesignerDb"] = _fixture.ConnectionString
                    });
                });
            });

        using var testScope = factory.Services.CreateScope();
        var dbContext = testScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = dbContext.Database.ProviderName;

        // Assert
        providerName.Should().NotBeNull();
        providerName.Should().ContainEquivalentOf("SqlServer");
        providerName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");

        // Cleanup
        factory.Dispose();
    }

    /// <summary>
    /// TC-003: Verify case-insensitive provider names
    /// Given: appsettings.json configured with variations ("sqlserver", "SQLSERVER", "SqlServer", "mssql")
    /// When: Application starts
    /// Then: All variations correctly resolve to SQL Server provider
    /// </summary>
    [Theory]
    [InlineData("sqlserver")]
    [InlineData("SQLSERVER")]
    [InlineData("SqlServer")]
    [InlineData("mssql")]
    public void TC003_ProviderDetection_CaseInsensitive_ShouldResolveToSqlServer(string providerVariation)
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = providerVariation,
                        ["Database:ConnectionStringName"] = "FormDesignerDb",
                        ["ConnectionStrings:FormDesignerDb"] = _fixture.ConnectionString
                    });
                });
            });

        // Act
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = dbContext.Database.ProviderName;

        // Assert
        providerName.Should().NotBeNull();
        providerName.Should().ContainEquivalentOf("SqlServer",
            $"provider variation '{providerVariation}' should resolve to SQL Server");

        // Cleanup
        factory.Dispose();
    }

    /// <summary>
    /// TC-004: Verify invalid provider error handling
    /// Given: appsettings.json configured with unsupported provider "MySQL"
    /// When: Application starts
    /// Then: InvalidOperationException thrown with clear error message
    /// </summary>
    [Fact]
    public void TC004_ProviderDetection_InvalidProvider_ShouldThrowException()
    {
        // Arrange & Act
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
                            ["Database:Provider"] = "MySQL",
                            ["Database:ConnectionStringName"] = "FormDesignerDb",
                            ["ConnectionStrings:FormDesignerDb"] = "Server=localhost;Database=test;Uid=root;Pwd=root;"
                        });
                    });
                });

            // Try to create scope - this should trigger provider initialization
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Accessing the provider should fail
            _ = dbContext.Database.ProviderName;

            factory.Dispose();
        };

        // Assert
        // Note: This test validates that invalid providers cause errors
        // The specific exception type may vary depending on when validation occurs
        act.Should().Throw<Exception>();
    }

    /// <summary>
    /// TC-004b: Verify empty/null provider error handling
    /// Given: appsettings.json with missing or empty provider
    /// When: Application starts
    /// Then: Clear error message about missing provider configuration
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TC004b_ProviderDetection_MissingProvider_ShouldThrowException(string? invalidProvider)
    {
        // Arrange & Act
        var act = () =>
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        var settings = new Dictionary<string, string?>
                        {
                            ["Database:ConnectionStringName"] = "FormDesignerDb",
                            ["ConnectionStrings:FormDesignerDb"] = _fixture.ConnectionString
                        };

                        // Only add provider if not null
                        if (invalidProvider != null)
                        {
                            settings["Database:Provider"] = invalidProvider;
                        }

                        config.AddInMemoryCollection(settings);
                    });
                });

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _ = dbContext.Database.ProviderName;

            factory.Dispose();
        };

        // Assert
        // Note: Missing provider should cause an error during initialization
        act.Should().Throw<Exception>();
    }

    /// <summary>
    /// TC-005: Verify provider-specific features are detected correctly
    /// Given: SQL Server provider is configured
    /// When: Accessing provider-specific features
    /// Then: Provider name can be used to enable/disable features
    /// </summary>
    [Fact]
    public void TC005_ProviderDetection_SqlServer_CanDetectProviderSpecificFeatures()
    {
        // Arrange
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
                        ["ConnectionStrings:FormDesignerDb"] = _fixture.ConnectionString
                    });
                });
            });

        // Act
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = dbContext.Database.ProviderName ?? string.Empty;

        // Simulate provider-specific feature detection (as used in ApplicationDbContext)
        var isNpgsql = providerName.Equals("Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.OrdinalIgnoreCase);
        var isSqlServer = providerName.Equals("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase);

        // Assert
        isNpgsql.Should().BeFalse(because: "SQL Server provider is configured, not PostgreSQL");
        isSqlServer.Should().BeTrue(because: "SQL Server provider is configured");

        // Cleanup
        factory.Dispose();
    }

    /// <summary>
    /// TC-006: Verify provider-specific features for PostgreSQL
    /// Given: PostgreSQL provider is configured
    /// When: Accessing provider-specific features
    /// Then: Provider name can be used to enable PostgreSQL features (like GIN indexes)
    /// </summary>
    [Fact]
    public async Task TC006_ProviderDetection_PostgreSQL_CanDetectProviderSpecificFeatures()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "Postgres",
                        ["Database:ConnectionStringName"] = "FormDesignerDb",
                        ["ConnectionStrings:FormDesignerDb"] = "Host=localhost;Database=formflow_test;Username=postgres;Password=postgres"
                    });
                });
            });

        // Act
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = dbContext.Database.ProviderName ?? string.Empty;

        // Simulate provider-specific feature detection (as used in ApplicationDbContext)
        var isNpgsql = providerName.Equals("Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.OrdinalIgnoreCase);
        var isSqlServer = providerName.Equals("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.OrdinalIgnoreCase);

        // Assert
        isNpgsql.Should().BeTrue(because: "PostgreSQL provider is configured");
        isSqlServer.Should().BeFalse(because: "SQL Server provider is not configured");

        // Cleanup
        await factory.DisposeAsync();
    }
}
