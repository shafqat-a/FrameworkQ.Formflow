using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a table column.
/// Similar to FieldSpec but includes formula support for computed columns.
/// </summary>
public class ColumnSpec
{
    /// <summary>
    /// Column name (unique identifier for data binding)
    /// </summary>
    [Required(ErrorMessage = "Column name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Column label displayed in table header
    /// </summary>
    [Required(ErrorMessage = "Column label is required")]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Data type: string, text, integer, decimal, date, time, datetime, bool, enum
    /// </summary>
    [Required(ErrorMessage = "Column type is required")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the column is required
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>
    /// Whether the column is read-only
    /// </summary>
    [JsonPropertyName("readonly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Readonly { get; set; }

    /// <summary>
    /// Unit label (e.g., "kg", "USD", "%")
    /// </summary>
    [JsonPropertyName("unit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Unit { get; set; }

    /// <summary>
    /// Validation pattern (regex)
    /// </summary>
    [JsonPropertyName("pattern")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Pattern { get; set; }

    /// <summary>
    /// Minimum value (for numeric/date types)
    /// </summary>
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Min { get; set; }

    /// <summary>
    /// Maximum value (for numeric/date types)
    /// </summary>
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Max { get; set; }

    /// <summary>
    /// Enum values (for type = "enum")
    /// </summary>
    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Enum { get; set; }

    /// <summary>
    /// Default value for the column
    /// </summary>
    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Default { get; set; }

    /// <summary>
    /// Formula expression for computed columns (e.g., "quantity * unit_price")
    /// Will be translated to SQL GENERATED ALWAYS AS during SQL generation
    /// </summary>
    [JsonPropertyName("formula")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Formula { get; set; }

    /// <summary>
    /// Format specification
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Format { get; set; }
}
