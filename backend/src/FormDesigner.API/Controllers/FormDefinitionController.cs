using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FormDesigner.API.Models.DTOs;
using FormDesigner.API.Services;

namespace FormDesigner.API.Controllers;

/// <summary>
/// Controller for form definition operations (design mode).
/// </summary>
[ApiController]
[Route("api/forms")]
public class FormDefinitionController : ControllerBase
{
    private readonly IFormBuilderService _formBuilderService;
    private readonly IValidator<FormDefinitionRoot> _validator;
    private readonly ILogger<FormDefinitionController> _logger;

    public FormDefinitionController(
        IFormBuilderService formBuilderService,
        IValidator<FormDefinitionRoot> validator,
        ILogger<FormDefinitionController> logger)
    {
        _formBuilderService = formBuilderService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Get all forms.
    /// </summary>
    /// <param name="includeInactive">Include soft-deleted forms</param>
    /// <returns>List of forms</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("GET /api/forms (includeInactive: {IncludeInactive})", includeInactive);

        var forms = await _formBuilderService.GetAllFormsAsync(includeInactive);
        return Ok(forms);
    }

    /// <summary>
    /// Get form by ID.
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <returns>Form definition</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("GET /api/forms/{Id}", id);

        var form = await _formBuilderService.GetFormByIdAsync(id);
        if (form == null)
        {
            return NotFound(new { message = $"Form '{id}' not found" });
        }

        return Ok(form);
    }

    /// <summary>
    /// Create a new form.
    /// </summary>
    /// <param name="formDefinitionRoot">Form definition</param>
    /// <returns>Created form</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] FormDefinitionRoot formDefinitionRoot)
    {
        _logger.LogInformation("POST /api/forms");

        // Validate
        var validationResult = await _validator.ValidateAsync(formDefinitionRoot);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new
            {
                message = "Validation failed",
                errors = errors
            });
        }

        try
        {
            var created = await _formBuilderService.CreateFormAsync(formDefinitionRoot);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Form.Id },
                created
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing form.
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <param name="formDefinitionRoot">Updated form definition</param>
    /// <returns>Updated form</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] FormDefinitionRoot formDefinitionRoot)
    {
        _logger.LogInformation("PUT /api/forms/{Id}", id);

        // Validate
        var validationResult = await _validator.ValidateAsync(formDefinitionRoot);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new
            {
                message = "Validation failed",
                errors = errors
            });
        }

        // Validate ID match
        if (id != formDefinitionRoot.Form.Id)
        {
            return BadRequest(new
            {
                message = $"Form ID in path '{id}' does not match ID in body '{formDefinitionRoot.Form.Id}'"
            });
        }

        try
        {
            var updated = await _formBuilderService.UpdateFormAsync(id, formDefinitionRoot);
            return Ok(updated);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("committed"))
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a form (soft delete).
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("DELETE /api/forms/{Id}", id);

        var result = await _formBuilderService.DeleteFormAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Form '{id}' not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Validate a form definition.
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <param name="formDefinitionRoot">Form definition to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("{id}/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(string id, [FromBody] FormDefinitionRoot formDefinitionRoot)
    {
        _logger.LogInformation("POST /api/forms/{Id}/validate", id);

        var validationResult = await _validator.ValidateAsync(formDefinitionRoot);

        if (validationResult.IsValid)
        {
            return Ok(new { isValid = true, message = "Form is valid" });
        }

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return BadRequest(new
        {
            isValid = false,
            message = "Validation failed",
            errors = errors
        });
    }

    /// <summary>
    /// Commit a form (lock for editing, make available for runtime).
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <returns>Success message</returns>
    [HttpPost("{id}/commit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Commit(string id)
    {
        _logger.LogInformation("POST /api/forms/{Id}/commit", id);

        var result = await _formBuilderService.CommitFormAsync(id);
        if (!result)
        {
            return Conflict(new { message = $"Form '{id}' not found or already committed" });
        }

        return Ok(new { message = $"Form '{id}' committed successfully" });
    }
}
