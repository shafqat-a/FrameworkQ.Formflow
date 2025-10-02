using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Data.Repositories;

/// <summary>
/// Repository interface for form instance operations (runtime mode).
/// </summary>
public interface IFormInstanceRepository
{
    /// <summary>
    /// Create a new form instance for data entry.
    /// </summary>
    /// <param name="formId">ID of the committed form</param>
    /// <param name="userId">Optional user identifier</param>
    /// <returns>Created form instance</returns>
    Task<FormInstanceEntity> CreateInstanceAsync(string formId, string? userId = null);

    /// <summary>
    /// Get form instance by ID.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <returns>Form instance or null if not found</returns>
    Task<FormInstanceEntity?> GetInstanceAsync(Guid instanceId);

    /// <summary>
    /// Get all instances for a specific form.
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="userId">Optional filter by user</param>
    /// <returns>List of form instances</returns>
    Task<List<FormInstanceEntity>> GetInstancesByFormIdAsync(string formId, string? userId = null);

    /// <summary>
    /// Save temporary progress for a form instance.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <param name="dataJson">Partial form data in JSON format</param>
    /// <param name="userId">Optional user identifier</param>
    /// <returns>Created temporary state</returns>
    Task<TemporaryStateEntity> SaveProgressAsync(Guid instanceId, string dataJson, string? userId = null);

    /// <summary>
    /// Get the latest temporary state for an instance.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <returns>Latest temporary state or null</returns>
    Task<TemporaryStateEntity?> GetLatestProgressAsync(Guid instanceId);

    /// <summary>
    /// Submit form instance (mark as submitted and store data).
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <param name="dataJson">Complete form data in JSON format</param>
    /// <returns>Updated form instance</returns>
    Task<FormInstanceEntity> SubmitInstanceAsync(Guid instanceId, string dataJson);

    /// <summary>
    /// Delete a form instance (for draft instances only).
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <returns>True if deleted, false if not found or already submitted</returns>
    Task<bool> DeleteInstanceAsync(Guid instanceId);
}
