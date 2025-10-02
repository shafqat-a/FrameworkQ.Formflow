using FormDesigner.API.Models.DTOs;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Services;

/// <summary>
/// Service interface for form builder operations (design mode).
/// </summary>
public interface IFormBuilderService
{
    /// <summary>
    /// Get all forms.
    /// </summary>
    /// <param name="includeInactive">If true, includes soft-deleted forms</param>
    /// <returns>List of form definition entities</returns>
    Task<List<FormDefinitionEntity>> GetAllFormsAsync(bool includeInactive = false);

    /// <summary>
    /// Get form by ID.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="includeInactive">If true, includes soft-deleted forms</param>
    /// <returns>Form definition root DTO or null if not found</returns>
    Task<FormDefinitionRoot?> GetFormByIdAsync(string formId, bool includeInactive = false);

    /// <summary>
    /// Create a new form definition.
    /// </summary>
    /// <param name="formDefinitionRoot">Form definition to create</param>
    /// <returns>Created form definition root DTO</returns>
    Task<FormDefinitionRoot> CreateFormAsync(FormDefinitionRoot formDefinitionRoot);

    /// <summary>
    /// Update an existing form definition.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="formDefinitionRoot">Updated form definition</param>
    /// <returns>Updated form definition root DTO</returns>
    Task<FormDefinitionRoot> UpdateFormAsync(string formId, FormDefinitionRoot formDefinitionRoot);

    /// <summary>
    /// Delete a form definition (soft delete).
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteFormAsync(string formId);

    /// <summary>
    /// Validate a form definition.
    /// </summary>
    /// <param name="formDefinitionRoot">Form definition to validate</param>
    /// <returns>Validation result with errors if any</returns>
    Task<ValidationResult> ValidateFormAsync(FormDefinitionRoot formDefinitionRoot);

    /// <summary>
    /// Commit a form definition (lock for editing, make available for runtime).
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>True if committed, false if not found or already committed</returns>
    Task<bool> CommitFormAsync(string formId);

    /// <summary>
    /// Get all committed forms available for runtime.
    /// </summary>
    /// <returns>List of committed form definition entities</returns>
    Task<List<FormDefinitionEntity>> GetCommittedFormsAsync();
}

/// <summary>
/// Validation result.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}
