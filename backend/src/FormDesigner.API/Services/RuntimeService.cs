using FormDesigner.API.Data.Repositories;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Services;

/// <summary>
/// Service implementation for runtime form execution operations.
/// </summary>
public class RuntimeService : IRuntimeService
{
    private readonly IFormRepository _formRepository;
    private readonly IFormInstanceRepository _instanceRepository;
    private readonly ILogger<RuntimeService> _logger;

    public RuntimeService(
        IFormRepository formRepository,
        IFormInstanceRepository instanceRepository,
        ILogger<RuntimeService> logger)
    {
        _formRepository = formRepository;
        _instanceRepository = instanceRepository;
        _logger = logger;
    }

    public async Task<List<FormDefinitionEntity>> GetCommittedFormsAsync()
    {
        _logger.LogInformation("Getting all committed forms for runtime");
        return await _formRepository.GetCommittedFormsAsync();
    }

    public async Task<FormInstanceEntity> CreateInstanceAsync(string formId, string? userId = null)
    {
        _logger.LogInformation("Creating instance for form: {FormId}", formId);

        // Verify form exists and is committed
        var form = await _formRepository.GetByIdAsync(formId);
        if (form == null)
        {
            _logger.LogWarning("Form not found: {FormId}", formId);
            throw new InvalidOperationException($"Form '{formId}' not found");
        }

        if (!form.IsCommitted)
        {
            _logger.LogWarning("Form not committed: {FormId}", formId);
            throw new InvalidOperationException($"Form '{formId}' is not committed and cannot be used for data entry");
        }

        var instance = await _instanceRepository.CreateInstanceAsync(formId, userId);
        _logger.LogInformation("Instance created: {InstanceId} for form: {FormId}", instance.InstanceId, formId);

        return instance;
    }

    public async Task<FormInstanceEntity?> GetInstanceAsync(Guid instanceId)
    {
        _logger.LogInformation("Getting instance: {InstanceId}", instanceId);
        return await _instanceRepository.GetInstanceAsync(instanceId);
    }

    public async Task<List<FormInstanceEntity>> GetInstancesByFormIdAsync(string formId, string? userId = null)
    {
        _logger.LogInformation("Getting instances for form: {FormId}", formId);
        return await _instanceRepository.GetInstancesByFormIdAsync(formId, userId);
    }

    public async Task<TemporaryStateEntity> SaveProgressAsync(Guid instanceId, string dataJson, string? userId = null)
    {
        _logger.LogInformation("Saving progress for instance: {InstanceId}", instanceId);

        // Verify instance exists and is draft
        var instance = await _instanceRepository.GetInstanceAsync(instanceId);
        if (instance == null)
        {
            _logger.LogWarning("Instance not found: {InstanceId}", instanceId);
            throw new InvalidOperationException($"Instance '{instanceId}' not found");
        }

        if (instance.Status == "submitted")
        {
            _logger.LogWarning("Cannot save progress for submitted instance: {InstanceId}", instanceId);
            throw new InvalidOperationException($"Instance '{instanceId}' is already submitted");
        }

        var state = await _instanceRepository.SaveProgressAsync(instanceId, dataJson, userId);
        _logger.LogInformation("Progress saved for instance: {InstanceId}, state: {StateId}", instanceId, state.StateId);

        return state;
    }

    public async Task<TemporaryStateEntity?> GetLatestProgressAsync(Guid instanceId)
    {
        _logger.LogInformation("Getting latest progress for instance: {InstanceId}", instanceId);
        return await _instanceRepository.GetLatestProgressAsync(instanceId);
    }

    public async Task<FormInstanceEntity> SubmitInstanceAsync(Guid instanceId, string dataJson)
    {
        _logger.LogInformation("Submitting instance: {InstanceId}", instanceId);

        // Verify instance exists
        var instance = await _instanceRepository.GetInstanceAsync(instanceId);
        if (instance == null)
        {
            _logger.LogWarning("Instance not found: {InstanceId}", instanceId);
            throw new InvalidOperationException($"Instance '{instanceId}' not found");
        }

        // TODO: Add data validation against form definition here
        // For now, just submit the data

        var submittedInstance = await _instanceRepository.SubmitInstanceAsync(instanceId, dataJson);
        _logger.LogInformation("Instance submitted successfully: {InstanceId}", instanceId);

        // TODO: Persist data to generated SQL tables
        // This would require:
        // 1. Parse form definition to find table/grid widgets
        // 2. Execute dynamic SQL INSERT statements
        // 3. Handle transactions

        return submittedInstance;
    }

    public async Task<bool> DeleteInstanceAsync(Guid instanceId)
    {
        _logger.LogInformation("Deleting instance: {InstanceId}", instanceId);

        var result = await _instanceRepository.DeleteInstanceAsync(instanceId);

        if (result)
        {
            _logger.LogInformation("Instance deleted successfully: {InstanceId}", instanceId);
        }
        else
        {
            _logger.LogWarning("Instance not found or already submitted: {InstanceId}", instanceId);
        }

        return result;
    }
}
