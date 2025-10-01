using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Data.Repositories;

/// <summary>
/// Repository interface for form definition operations (design mode).
/// </summary>
public interface IFormRepository
{
    /// <summary>
    /// Get all form definitions.
    /// </summary>
    /// <param name="includeInactive">If true, includes soft-deleted forms</param>
    /// <returns>List of form definitions</returns>
    Task<List<FormDefinitionEntity>> GetAllAsync(bool includeInactive = false);

    /// <summary>
    /// Get form definition by ID.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="includeInactive">If true, includes soft-deleted forms</param>
    /// <returns>Form definition or null if not found</returns>
    Task<FormDefinitionEntity?> GetByIdAsync(string formId, bool includeInactive = false);

    /// <summary>
    /// Create a new form definition.
    /// </summary>
    /// <param name="formDefinition">Form definition to create</param>
    /// <returns>Created form definition</returns>
    Task<FormDefinitionEntity> CreateAsync(FormDefinitionEntity formDefinition);

    /// <summary>
    /// Update an existing form definition.
    /// </summary>
    /// <param name="formDefinition">Form definition to update</param>
    /// <returns>Updated form definition</returns>
    Task<FormDefinitionEntity> UpdateAsync(FormDefinitionEntity formDefinition);

    /// <summary>
    /// Soft delete a form definition (sets IsActive = false).
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string formId);

    /// <summary>
    /// Check if a form definition exists.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string formId);

    /// <summary>
    /// Commit a form definition (set IsCommitted = true).
    /// Committed forms are locked for editing and available for runtime.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <returns>True if committed, false if not found or already committed</returns>
    Task<bool> CommitAsync(string formId);

    /// <summary>
    /// Get all committed forms available for runtime.
    /// </summary>
    /// <returns>List of committed form definitions</returns>
    Task<List<FormDefinitionEntity>> GetCommittedFormsAsync();
}
