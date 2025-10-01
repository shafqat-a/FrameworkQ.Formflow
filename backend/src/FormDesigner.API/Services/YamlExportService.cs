using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FormDesigner.API.Data.Repositories;
using FormDesigner.API.Models.DTOs;

namespace FormDesigner.API.Services;

/// <summary>
/// Service implementation for exporting forms to YAML format.
/// Produces YAML compliant with DSL v0.1 specification.
/// </summary>
public class YamlExportService : IYamlExportService
{
    private readonly IFormRepository _formRepository;
    private readonly ILogger<YamlExportService> _logger;
    private static readonly ISerializer _yamlSerializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public YamlExportService(IFormRepository formRepository, ILogger<YamlExportService> logger)
    {
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<string?> ExportToYamlAsync(string formId)
    {
        _logger.LogInformation("Exporting form to YAML: {FormId}", formId);

        var entity = await _formRepository.GetByIdAsync(formId);
        if (entity == null)
        {
            _logger.LogWarning("Form not found for YAML export: {FormId}", formId);
            return null;
        }

        // Deserialize JSON to object
        var formRoot = JsonSerializer.Deserialize<FormDefinitionRoot>(entity.DslJson, _jsonOptions);
        if (formRoot == null)
        {
            _logger.LogError("Failed to deserialize form JSON: {FormId}", formId);
            return null;
        }

        // Serialize to YAML with snake_case and null omission
        var yaml = _yamlSerializer.Serialize(formRoot);

        _logger.LogInformation("Form exported to YAML successfully: {FormId}", formId);
        return yaml;
    }
}
