using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FormDesigner.API.Models.DTOs;

namespace FormDesigner.API.Services;

/// <summary>
/// Service implementation for importing forms from YAML format.
/// </summary>
public class YamlImportService : IYamlImportService
{
    private readonly IFormBuilderService _formBuilderService;
    private readonly ILogger<YamlImportService> _logger;
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public YamlImportService(IFormBuilderService formBuilderService, ILogger<YamlImportService> logger)
    {
        _formBuilderService = formBuilderService;
        _logger = logger;
    }

    public async Task<FormDefinitionRoot> ImportFromYamlAsync(Stream yamlStream)
    {
        _logger.LogInformation("Importing form from YAML");

        // Read YAML content
        string yamlContent;
        using (var reader = new StreamReader(yamlStream))
        {
            yamlContent = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            _logger.LogError("Empty YAML content");
            throw new InvalidOperationException("YAML content is empty");
        }

        // Deserialize YAML to FormDefinitionRoot
        FormDefinitionRoot? formRoot;
        try
        {
            formRoot = _yamlDeserializer.Deserialize<FormDefinitionRoot>(yamlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse YAML");
            throw new InvalidOperationException("Invalid YAML format", ex);
        }

        if (formRoot?.Form == null)
        {
            _logger.LogError("YAML does not contain valid form structure");
            throw new InvalidOperationException("YAML does not contain valid form structure");
        }

        var formId = formRoot.Form.Id;
        _logger.LogInformation("Parsed form from YAML: {FormId}", formId);

        // Check if form exists
        var existingForm = await _formBuilderService.GetFormByIdAsync(formId);

        if (existingForm != null)
        {
            // Update existing form (only if not committed)
            _logger.LogInformation("Updating existing form from YAML: {FormId}", formId);
            return await _formBuilderService.UpdateFormAsync(formId, formRoot);
        }
        else
        {
            // Create new form
            _logger.LogInformation("Creating new form from YAML: {FormId}", formId);
            return await _formBuilderService.CreateFormAsync(formRoot);
        }
    }
}
