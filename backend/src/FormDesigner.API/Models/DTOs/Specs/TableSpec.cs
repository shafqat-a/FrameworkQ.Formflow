using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a table widget with repeating rows.
/// </summary>
public class TableSpec
{
    /// <summary>
    /// Row mode: "finite" (fixed number) or "infinite" (unlimited)
    /// </summary>
    [JsonPropertyName("row_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RowMode { get; set; }

    /// <summary>
    /// Minimum number of rows (for finite mode)
    /// </summary>
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Min { get; set; }

    /// <summary>
    /// Maximum number of rows (for finite mode)
    /// </summary>
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Max { get; set; }

    /// <summary>
    /// Row key field name for unique identification
    /// </summary>
    [JsonPropertyName("row_key")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RowKey { get; set; }

    /// <summary>
    /// Table column definitions
    /// </summary>
    [Required(ErrorMessage = "Table must have columns")]
    [MinLength(1, ErrorMessage = "Table must have at least one column")]
    [JsonPropertyName("columns")]
    public List<ColumnSpec> Columns { get; set; } = new();

    /// <summary>
    /// Row generators for dynamic row creation
    /// </summary>
    [JsonPropertyName("row_generators")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<RowGenerator>? RowGenerators { get; set; }

    /// <summary>
    /// Aggregate calculations (sum, avg, etc.)
    /// </summary>
    [JsonPropertyName("aggregates")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object>>? Aggregates { get; set; }
}

/// <summary>
/// Row generator specification for creating rows dynamically.
/// </summary>
public class RowGenerator
{
    /// <summary>
    /// Generator type: "range" or "values"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Source data type: "integer", "date", "time", "datetime"
    /// </summary>
    [JsonPropertyName("from_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FromType { get; set; }

    /// <summary>
    /// Start value for range
    /// </summary>
    [JsonPropertyName("from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? From { get; set; }

    /// <summary>
    /// End value for range
    /// </summary>
    [JsonPropertyName("to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? To { get; set; }

    /// <summary>
    /// Step/increment value
    /// </summary>
    [JsonPropertyName("step")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Step { get; set; }

    /// <summary>
    /// List of values (for type = "values")
    /// </summary>
    [JsonPropertyName("values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Values { get; set; }
}
