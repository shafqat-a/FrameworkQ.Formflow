namespace FormDesigner.API.Services;

/// <summary>
/// Service interface for exporting forms to YAML format.
/// </summary>
public interface IYamlExportService
{
    /// <summary>
    /// Export form definition to YAML string.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>YAML string or null if form not found</returns>
    Task<string?> ExportToYamlAsync(string formId);
}
