using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for CheckboxGroup widget - multiple selection from options.
/// Used for multi-select choices with optional min/max constraints.
/// </summary>
public class CheckboxGroupSpec
{
    /// <summary>
    /// Checkbox group options
    /// </summary>
    [Required]
    [MinLength(1)]
    [JsonPropertyName("options")]
    public List<CheckboxOption> Options { get; set; } = new();

    /// <summary>
    /// Layout orientation (horizontal, vertical, grid)
    /// </summary>
    [JsonPropertyName("orientation")]
    public string Orientation { get; set; } = "vertical";

    /// <summary>
    /// Minimum number of selections required
    /// </summary>
    [JsonPropertyName("min_selections")]
    public int? MinSelections { get; set; }

    /// <summary>
    /// Maximum number of selections allowed
    /// </summary>
    [JsonPropertyName("max_selections")]
    public int? MaxSelections { get; set; }

    /// <summary>
    /// Default selected values
    /// </summary>
    [JsonPropertyName("default_values")]
    public List<string>? DefaultValues { get; set; }

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Number of columns for grid layout
    /// </summary>
    [JsonPropertyName("grid_columns")]
    public int GridColumns { get; set; } = 2;

    /// <summary>
    /// Whether to show "Select All" / "Deselect All" buttons
    /// </summary>
    [JsonPropertyName("show_select_all")]
    public bool ShowSelectAll { get; set; } = false;

    /// <summary>
    /// Spacing between options in pixels
    /// </summary>
    [JsonPropertyName("spacing")]
    public int Spacing { get; set; } = 10;
}

/// <summary>
/// Represents a single checkbox option
/// </summary>
public class CheckboxOption
{
    /// <summary>
    /// Option value (stored in database)
    /// </summary>
    [Required]
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Option label (displayed to user)
    /// </summary>
    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Option icon (optional)
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Custom color for option
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// Whether option is disabled
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Option description/help text
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
