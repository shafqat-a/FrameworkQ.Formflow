using System.Text;
using System.Text.Json;
using FormDesigner.API.Data.Repositories;
using FormDesigner.API.Models.DTOs;
using FormDesigner.API.Models.DTOs.Specs;

namespace FormDesigner.API.Services;

/// <summary>
/// Service implementation for generating SQL DDL from form definitions.
/// Generates PostgreSQL-compatible CREATE TABLE statements.
/// </summary>
public class SqlGeneratorService : ISqlGeneratorService
{
    private readonly IFormRepository _formRepository;
    private readonly ILogger<SqlGeneratorService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public SqlGeneratorService(IFormRepository formRepository, ILogger<SqlGeneratorService> logger)
    {
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<string?> GenerateSqlAsync(string formId)
    {
        _logger.LogInformation("Generating SQL DDL for form: {FormId}", formId);

        var entity = await _formRepository.GetByIdAsync(formId);
        if (entity == null)
        {
            _logger.LogWarning("Form not found for SQL generation: {FormId}", formId);
            return null;
        }

        var formRoot = JsonSerializer.Deserialize<FormDefinitionRoot>(entity.DslJson, _jsonOptions);
        if (formRoot == null)
        {
            _logger.LogError("Failed to deserialize form JSON: {FormId}", formId);
            return null;
        }

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"-- SQL DDL generated for form: {formId}");
        sqlBuilder.AppendLine($"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sqlBuilder.AppendLine();

        int tableCount = 0;

        // Iterate through pages, sections, and widgets
        foreach (var page in formRoot.Form.Pages)
        {
            foreach (var section in page.Sections)
            {
                foreach (var widget in section.Widgets)
                {
                    if (widget.Type == "table" && widget.Table != null)
                    {
                        GenerateTableSql(sqlBuilder, formId, page.Id, section.Id, widget.Id, widget.Table);
                        tableCount++;
                    }
                    else if (widget.Type == "grid" && widget.Grid != null)
                    {
                        GenerateGridSql(sqlBuilder, formId, page.Id, section.Id, widget.Id, widget.Grid);
                        tableCount++;
                    }
                }
            }
        }

        if (tableCount == 0)
        {
            sqlBuilder.AppendLine("-- No table or grid widgets found in this form");
        }

        _logger.LogInformation("SQL DDL generated successfully for form {FormId}: {TableCount} tables", formId, tableCount);
        return sqlBuilder.ToString();
    }

    private void GenerateTableSql(StringBuilder sqlBuilder, string formId, string pageId, string sectionId, string widgetId, TableSpec table)
    {
        var tableName = $"{formId}_{pageId}_{sectionId}_{widgetId}";

        sqlBuilder.AppendLine($"-- Table for widget: {widgetId}");
        sqlBuilder.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        // Standard columns
        sqlBuilder.AppendLine("    instance_id UUID NOT NULL,");
        sqlBuilder.AppendLine("    page_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    section_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    widget_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    row_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),");

        // Data columns from table definition
        foreach (var column in table.Columns)
        {
            var columnDef = GenerateColumnDefinition(column);
            sqlBuilder.AppendLine($"    {columnDef},");
        }

        // Primary key
        sqlBuilder.AppendLine("    PRIMARY KEY (instance_id, row_id)");
        sqlBuilder.AppendLine(");");
        sqlBuilder.AppendLine();

        // Indexes
        sqlBuilder.AppendLine($"CREATE INDEX IF NOT EXISTS idx_{tableName}_instance ON {tableName}(instance_id);");
        sqlBuilder.AppendLine($"CREATE INDEX IF NOT EXISTS idx_{tableName}_recorded ON {tableName}(recorded_at);");
        sqlBuilder.AppendLine();
    }

    private void GenerateGridSql(StringBuilder sqlBuilder, string formId, string pageId, string sectionId, string widgetId, GridSpec grid)
    {
        var tableName = $"{formId}_{pageId}_{sectionId}_{widgetId}";

        sqlBuilder.AppendLine($"-- Grid table for widget: {widgetId}");
        sqlBuilder.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        // Standard columns
        sqlBuilder.AppendLine("    instance_id UUID NOT NULL,");
        sqlBuilder.AppendLine("    page_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    section_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    widget_id TEXT NOT NULL,");
        sqlBuilder.AppendLine("    row_key TEXT NOT NULL,");
        sqlBuilder.AppendLine("    column_key TEXT NOT NULL,");
        sqlBuilder.AppendLine("    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),");

        // Cell value column
        var cellType = grid.Cell?.Type ?? "string";
        var sqlType = MapDslTypeToSql(cellType);
        sqlBuilder.AppendLine($"    cell_value {sqlType},");

        // Primary key
        sqlBuilder.AppendLine("    PRIMARY KEY (instance_id, row_key, column_key)");
        sqlBuilder.AppendLine(");");
        sqlBuilder.AppendLine();

        // Indexes
        sqlBuilder.AppendLine($"CREATE INDEX IF NOT EXISTS idx_{tableName}_instance ON {tableName}(instance_id);");
        sqlBuilder.AppendLine();
    }

    private string GenerateColumnDefinition(ColumnSpec column)
    {
        var sqlType = MapDslTypeToSql(column.Type);

        // Handle computed columns with formulas
        if (!string.IsNullOrEmpty(column.Formula))
        {
            var sqlFormula = TranslateFormulaToSql(column.Formula);
            return $"{column.Name} {sqlType} GENERATED ALWAYS AS ({sqlFormula}) STORED";
        }

        var notNull = column.Required == true ? " NOT NULL" : "";
        var defaultValue = column.Default != null ? $" DEFAULT {FormatDefaultValue(column.Default)}" : "";

        return $"{column.Name} {sqlType}{notNull}{defaultValue}";
    }

    private string MapDslTypeToSql(string dslType)
    {
        return dslType.ToLower() switch
        {
            "string" => "VARCHAR(255)",
            "text" => "TEXT",
            "integer" => "INTEGER",
            "decimal" => "NUMERIC(18,2)",
            "date" => "DATE",
            "time" => "TIME",
            "datetime" => "TIMESTAMPTZ",
            "bool" => "BOOLEAN",
            "enum" => "VARCHAR(100)",
            _ => "TEXT"
        };
    }

    private string TranslateFormulaToSql(string formula)
    {
        // Basic formula translation: wrap column references with COALESCE for null safety
        // Example: "quantity * unit_price" -> "COALESCE(quantity, 0) * COALESCE(unit_price, 0)"

        // Simple regex to find column names (alphanumeric + underscore)
        var pattern = @"\b([a-z_][a-z0-9_]*)\b";
        var translated = System.Text.RegularExpressions.Regex.Replace(formula, pattern, match =>
        {
            var columnName = match.Groups[1].Value;
            // Skip SQL keywords and functions
            if (IsSqlKeyword(columnName))
            {
                return columnName;
            }
            return $"COALESCE({columnName}, 0)";
        });

        return translated;
    }

    private bool IsSqlKeyword(string word)
    {
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or", "not", "null", "true", "false", "sum", "avg", "count", "max", "min"
        };
        return keywords.Contains(word);
    }

    private string FormatDefaultValue(object value)
    {
        if (value is string str)
        {
            return $"'{str.Replace("'", "''")}'";
        }
        if (value is bool b)
        {
            return b ? "TRUE" : "FALSE";
        }
        return value.ToString() ?? "NULL";
    }
}
