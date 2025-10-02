using FormDesigner.API.Models.DTOs;

namespace FormDesigner.API.Services;

/// <summary>
/// Service interface for importing forms from YAML format.
/// </summary>
public interface IYamlImportService
{
    /// <summary>
    /// Import form definition from YAML stream.
    /// Creates new form or updates existing draft form.
    /// </summary>
    /// <param name="yamlStream">YAML file stream</param>
    /// <returns>Imported form definition root</returns>
    Task<FormDefinitionRoot> ImportFromYamlAsync(Stream yamlStream);
}
