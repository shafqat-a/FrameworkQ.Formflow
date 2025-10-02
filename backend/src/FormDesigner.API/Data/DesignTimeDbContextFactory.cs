using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FormDesigner.API.Data;

/// <summary>
/// Design-time factory for creating DbContext during migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use connection string for migrations
        optionsBuilder.UseNpgsql("Host=localhost;Port=5400;Database=formflow;Username=postgres;Password=orion@123");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
