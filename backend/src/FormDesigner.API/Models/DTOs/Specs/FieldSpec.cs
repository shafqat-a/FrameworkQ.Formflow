using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a single field widget or field within a group.
/// Defines data type, validation rules, and presentation properties.
/// </summary>
public class FieldSpec
{
    /// <summary>
    /// Field name (unique identifier for data binding)
    /// </summary>
    [Required(ErrorMessage = "Field name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Field label displayed to users
    /// </summary>
    [Required(ErrorMessage = "Field label is required")]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Data type: string, text, integer, decimal, date, time, datetime, bool, enum, attachment, signature
    /// </summary>
    [Required(ErrorMessage = "Field type is required")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the field is required for form submission
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>
    /// Whether the field is read-only
    /// </summary>
    [JsonPropertyName("readonly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Readonly { get; set; }

    /// <summary>
    /// Placeholder text for empty fields
    /// </summary>
    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Default value for the field
    /// </summary>
    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Default { get; set; }

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
    /// Format specification
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Format { get; set; }

    /// <summary>
    /// Formula expression for computed fields
    /// </summary>
    [JsonPropertyName("compute")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Compute { get; set; }

    /// <summary>
    /// Override specifications
    /// </summary>
    [JsonPropertyName("override")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Override { get; set; }
}
