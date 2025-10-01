namespace FormDesigner.API.Services;

/// <summary>
/// Service interface for generating SQL DDL from form definitions.
/// </summary>
public interface ISqlGeneratorService
{
    /// <summary>
    /// Generate PostgreSQL DDL statements for a form's table and grid widgets.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>SQL DDL string or null if form not found</returns>
    Task<string?> GenerateSqlAsync(string formId);
}
