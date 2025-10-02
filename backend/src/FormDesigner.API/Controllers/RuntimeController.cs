using Microsoft.AspNetCore.Mvc;
using FormDesigner.API.Services;

namespace FormDesigner.API.Controllers;

/// <summary>
/// Controller for runtime form execution operations.
/// </summary>
[ApiController]
[Route("api/runtime")]
public class RuntimeController : ControllerBase
{
    private readonly IRuntimeService _runtimeService;
    private readonly ILogger<RuntimeController> _logger;

    public RuntimeController(
        IRuntimeService runtimeService,
        ILogger<RuntimeController> logger)
    {
        _runtimeService = runtimeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all committed forms available for runtime.
    /// </summary>
    /// <returns>List of committed forms</returns>
    [HttpGet("forms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommittedForms()
    {
        _logger.LogInformation("GET /api/runtime/forms");

        var forms = await _runtimeService.GetCommittedFormsAsync();
        return Ok(forms);
    }

    /// <summary>
    /// Create a new form instance for data entry.
    /// </summary>
    /// <param name="request">Instance creation request</param>
    /// <returns>Created instance</returns>
    [HttpPost("instances")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInstance([FromBody] CreateInstanceRequest request)
    {
        _logger.LogInformation("POST /api/runtime/instances for form: {FormId}", request.FormId);

        try
        {
            var instance = await _runtimeService.CreateInstanceAsync(request.FormId, request.UserId);
            return CreatedAtAction(
                nameof(GetInstance),
                new { instanceId = instance.InstanceId },
                instance
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not committed"))
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get form instance by ID.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <returns>Form instance</returns>
    [HttpGet("instances/{instanceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstance(Guid instanceId)
    {
        _logger.LogInformation("GET /api/runtime/instances/{InstanceId}", instanceId);

        var instance = await _runtimeService.GetInstanceAsync(instanceId);
        if (instance == null)
        {
            return NotFound(new { message = $"Instance '{instanceId}' not found" });
        }

        // Include latest progress if exists
        var latestProgress = await _runtimeService.GetLatestProgressAsync(instanceId);

        return Ok(new
        {
            instance = instance,
            latestProgress = latestProgress
        });
    }

    /// <summary>
    /// Save progress for a form instance (temporary state).
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <param name="request">Save progress request</param>
    /// <returns>Saved state</returns>
    [HttpPut("instances/{instanceId}/save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveProgress(Guid instanceId, [FromBody] SaveProgressRequest request)
    {
        _logger.LogInformation("PUT /api/runtime/instances/{InstanceId}/save", instanceId);

        try
        {
            var state = await _runtimeService.SaveProgressAsync(instanceId, request.DataJson, request.UserId);
            return Ok(new
            {
                message = "Progress saved successfully",
                state = state
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("submitted"))
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submit form instance.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <param name="request">Submit request</param>
    /// <returns>Submitted instance</returns>
    [HttpPost("instances/{instanceId}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid instanceId, [FromBody] SubmitRequest request)
    {
        _logger.LogInformation("POST /api/runtime/instances/{InstanceId}/submit", instanceId);

        try
        {
            var instance = await _runtimeService.SubmitInstanceAsync(instanceId, request.DataJson);
            return Ok(new
            {
                message = "Form submitted successfully",
                instance = instance
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a draft form instance.
    /// </summary>
    /// <param name="instanceId">Instance identifier</param>
    /// <returns>No content</returns>
    [HttpDelete("instances/{instanceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteInstance(Guid instanceId)
    {
        _logger.LogInformation("DELETE /api/runtime/instances/{InstanceId}", instanceId);

        var result = await _runtimeService.DeleteInstanceAsync(instanceId);
        if (!result)
        {
            return BadRequest(new { message = $"Instance '{instanceId}' not found or already submitted" });
        }

        return NoContent();
    }
}

/// <summary>
/// Request to create a form instance.
/// </summary>
public class CreateInstanceRequest
{
    public string FormId { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

/// <summary>
/// Request to save progress.
/// </summary>
public class SaveProgressRequest
{
    public string DataJson { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

/// <summary>
/// Request to submit form.
/// </summary>
public class SubmitRequest
{
    public string DataJson { get; set; } = string.Empty;
}
