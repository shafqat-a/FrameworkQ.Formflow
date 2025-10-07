using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FormDesigner.API.Data;

/// <summary>
/// Design-time factory for creating DbContext during migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = configuration["Database:Provider"]
            ?? configuration["DatabaseProvider"]
            ?? "Postgres";

        var connectionStringName = configuration["Database:ConnectionStringName"] ?? "FormDesignerDb";
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found for design-time DbContext creation.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        switch (provider.Trim().ToLowerInvariant())
        {
            case "postgres":
            case "postgresql":
            case "npgsql":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "sqlserver":
            case "mssql":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider '{provider}' for design-time configuration.");
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
