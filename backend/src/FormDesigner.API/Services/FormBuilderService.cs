using System.Text.Json;
using FormDesigner.API.Data.Repositories;
using FormDesigner.API.Models.DTOs;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Services;

/// <summary>
/// Service implementation for form builder operations (design mode).
/// </summary>
public class FormBuilderService : IFormBuilderService
{
    private readonly IFormRepository _formRepository;
    private readonly ILogger<FormBuilderService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public FormBuilderService(IFormRepository formRepository, ILogger<FormBuilderService> logger)
    {
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<List<FormDefinitionEntity>> GetAllFormsAsync(bool includeInactive = false)
    {
        _logger.LogInformation("Getting all forms (includeInactive: {IncludeInactive})", includeInactive);
        return await _formRepository.GetAllAsync(includeInactive);
    }

    public async Task<FormDefinitionRoot?> GetFormByIdAsync(string formId, bool includeInactive = false)
    {
        _logger.LogInformation("Getting form by ID: {FormId}", formId);

        var entity = await _formRepository.GetByIdAsync(formId, includeInactive);
        if (entity == null)
        {
            _logger.LogWarning("Form not found: {FormId}", formId);
            return null;
        }

        // Deserialize DslJson to FormDefinitionRoot
        var formRoot = JsonSerializer.Deserialize<FormDefinitionRoot>(entity.DslJson, _jsonOptions);
        return formRoot;
    }

    public async Task<FormDefinitionRoot> CreateFormAsync(FormDefinitionRoot formDefinitionRoot)
    {
        var formId = formDefinitionRoot.Form.Id;
        _logger.LogInformation("Creating form: {FormId}", formId);

        // Check for duplicate
        if (await _formRepository.ExistsAsync(formId))
        {
            _logger.LogWarning("Form already exists: {FormId}", formId);
            throw new InvalidOperationException($"Form with ID '{formId}' already exists");
        }

        // Serialize to JSON
        var dslJson = JsonSerializer.Serialize(formDefinitionRoot, _jsonOptions);

        var entity = new FormDefinitionEntity
        {
            FormId = formDefinitionRoot.Form.Id,
            Version = formDefinitionRoot.Form.Version,
            DslJson = dslJson,
            IsCommitted = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _formRepository.CreateAsync(entity);
        _logger.LogInformation("Form created successfully: {FormId}", formId);

        return formDefinitionRoot;
    }

    public async Task<FormDefinitionRoot> UpdateFormAsync(string formId, FormDefinitionRoot formDefinitionRoot)
    {
        _logger.LogInformation("Updating form: {FormId}", formId);

        // Validate ID match
        if (formId != formDefinitionRoot.Form.Id)
        {
            _logger.LogWarning("Form ID mismatch: {PathId} vs {BodyId}", formId, formDefinitionRoot.Form.Id);
            throw new InvalidOperationException($"Form ID in path '{formId}' does not match ID in body '{formDefinitionRoot.Form.Id}'");
        }

        var existingEntity = await _formRepository.GetByIdAsync(formId);
        if (existingEntity == null)
        {
            _logger.LogWarning("Form not found for update: {FormId}", formId);
            throw new InvalidOperationException($"Form with ID '{formId}' not found");
        }

        // Prevent editing committed forms
        if (existingEntity.IsCommitted)
        {
            _logger.LogWarning("Cannot edit committed form: {FormId}", formId);
            throw new InvalidOperationException($"Form '{formId}' is committed and cannot be edited");
        }

        // Serialize to JSON
        var dslJson = JsonSerializer.Serialize(formDefinitionRoot, _jsonOptions);

        existingEntity.Version = formDefinitionRoot.Form.Version;
        existingEntity.DslJson = dslJson;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        await _formRepository.UpdateAsync(existingEntity);
        _logger.LogInformation("Form updated successfully: {FormId}", formId);

        return formDefinitionRoot;
    }

    public async Task<bool> DeleteFormAsync(string formId)
    {
        _logger.LogInformation("Deleting form: {FormId}", formId);

        var result = await _formRepository.DeleteAsync(formId);

        if (result)
        {
            _logger.LogInformation("Form deleted successfully: {FormId}", formId);
        }
        else
        {
            _logger.LogWarning("Form not found for deletion: {FormId}", formId);
        }

        return result;
    }

    public async Task<ValidationResult> ValidateFormAsync(FormDefinitionRoot formDefinitionRoot)
    {
        _logger.LogInformation("Validating form: {FormId}", formDefinitionRoot.Form.Id);

        var validationResult = new ValidationResult { IsValid = true };

        // Basic validation (can be expanded with FluentValidation later)
        if (string.IsNullOrWhiteSpace(formDefinitionRoot.Form.Id))
        {
            validationResult.IsValid = false;
            AddError(validationResult, "form.id", "Form ID is required");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(formDefinitionRoot.Form.Id ?? "", @"^[a-z0-9_-]+$"))
        {
            validationResult.IsValid = false;
            AddError(validationResult, "form.id", "Form ID must contain only lowercase letters, numbers, hyphens, and underscores");
        }

        if (string.IsNullOrWhiteSpace(formDefinitionRoot.Form.Title))
        {
            validationResult.IsValid = false;
            AddError(validationResult, "form.title", "Form title is required");
        }

        if (formDefinitionRoot.Form.Pages == null || formDefinitionRoot.Form.Pages.Count == 0)
        {
            validationResult.IsValid = false;
            AddError(validationResult, "form.pages", "Form must have at least one page");
        }

        // TODO: Add more validation rules (duplicate widget IDs, etc.)

        return await Task.FromResult(validationResult);
    }

    public async Task<bool> CommitFormAsync(string formId)
    {
        _logger.LogInformation("Committing form: {FormId}", formId);

        var result = await _formRepository.CommitAsync(formId);

        if (result)
        {
            _logger.LogInformation("Form committed successfully: {FormId}", formId);
        }
        else
        {
            _logger.LogWarning("Form not found or already committed: {FormId}", formId);
        }

        return result;
    }

    public async Task<List<FormDefinitionEntity>> GetCommittedFormsAsync()
    {
        _logger.LogInformation("Getting all committed forms");
        return await _formRepository.GetCommittedFormsAsync();
    }

    private void AddError(ValidationResult result, string field, string message)
    {
        if (!result.Errors.ContainsKey(field))
        {
            result.Errors[field] = new List<string>();
        }
        result.Errors[field].Add(message);
    }
}
