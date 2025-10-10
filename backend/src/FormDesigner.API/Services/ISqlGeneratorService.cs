namespace FormDesigner.API.Services;

/// <summary>
/// Service for generating database-provider-specific SQL DDL from form definitions.
/// Supports both PostgreSQL and SQL Server.
/// </summary>
public interface ISqlGeneratorService
{
    /// <summary>
    /// Generates SQL DDL (CREATE TABLE statements) for table and grid widgets in a form.
    /// The generated SQL is specific to the configured database provider (PostgreSQL or SQL Server).
    /// </summary>
    /// <param name="formId">The unique identifier of the form</param>
    /// <returns>Generated SQL DDL string, or null if form not found</returns>
    /// <remarks>
    /// Provider detection is automatic based on the configured ApplicationDbContext.
    /// PostgreSQL: Uses VARCHAR, TEXT, TIMESTAMPTZ, BOOLEAN, UUID, GENERATED ALWAYS AS ... STORED
    /// SQL Server: Uses NVARCHAR, DATETIME2, BIT, UNIQUEIDENTIFIER, AS ... PERSISTED
    /// </remarks>
    Task<string?> GenerateSqlAsync(string formId);
}
