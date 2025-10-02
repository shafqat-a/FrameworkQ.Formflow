using Microsoft.AspNetCore.Mvc;
using FormDesigner.API.Services;

namespace FormDesigner.API.Controllers;

/// <summary>
/// Controller for export and import operations.
/// </summary>
[ApiController]
[Route("api")]
public class ExportController : ControllerBase
{
    private readonly IYamlExportService _yamlExportService;
    private readonly IYamlImportService _yamlImportService;
    private readonly ISqlGeneratorService _sqlGeneratorService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(
        IYamlExportService yamlExportService,
        IYamlImportService yamlImportService,
        ISqlGeneratorService sqlGeneratorService,
        ILogger<ExportController> logger)
    {
        _yamlExportService = yamlExportService;
        _yamlImportService = yamlImportService;
        _sqlGeneratorService = sqlGeneratorService;
        _logger = logger;
    }

    /// <summary>
    /// Export form definition to YAML.
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <returns>YAML file</returns>
    [HttpGet("export/{id}/yaml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportYaml(string id)
    {
        _logger.LogInformation("GET /api/export/{Id}/yaml", id);

        var yaml = await _yamlExportService.ExportToYamlAsync(id);
        if (yaml == null)
        {
            return NotFound(new { message = $"Form '{id}' not found" });
        }

        var fileName = $"{id}_form.yaml";
        return File(
            System.Text.Encoding.UTF8.GetBytes(yaml),
            "application/x-yaml",
            fileName
        );
    }

    /// <summary>
    /// Import form definition from YAML file.
    /// </summary>
    /// <param name="file">YAML file</param>
    /// <returns>Imported form</returns>
    [HttpPost("import/yaml")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportYaml(IFormFile file)
    {
        _logger.LogInformation("POST /api/import/yaml");

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        if (!file.FileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "File must be a YAML file (.yaml or .yml)" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var formRoot = await _yamlImportService.ImportFromYamlAsync(stream);

            // Return 201 if created, 200 if updated
            return Ok(new
            {
                message = $"Form '{formRoot.Form.Id}' imported successfully",
                form = formRoot
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Generate SQL DDL for form.
    /// </summary>
    /// <param name="id">Form identifier</param>
    /// <returns>SQL file</returns>
    [HttpGet("export/{id}/sql")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportSql(string id)
    {
        _logger.LogInformation("GET /api/export/{Id}/sql", id);

        var sql = await _sqlGeneratorService.GenerateSqlAsync(id);
        if (sql == null)
        {
            return NotFound(new { message = $"Form '{id}' not found" });
        }

        var fileName = $"{id}_schema.sql";
        return File(
            System.Text.Encoding.UTF8.GetBytes(sql),
            "text/plain",
            fileName
        );
    }
}
