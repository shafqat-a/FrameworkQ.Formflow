using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for RadioGroup widget - single selection from options.
/// Used for exclusive choices (e.g., Good/Acceptable/Poor, Yes/No).
/// </summary>
public class RadioGroupSpec
{
    /// <summary>
    /// Radio group options
    /// </summary>
    [Required]
    [MinLength(1)]
    [JsonPropertyName("options")]
    public List<RadioOption> Options { get; set; } = new();

    /// <summary>
    /// Layout orientation (horizontal, vertical)
    /// </summary>
    [JsonPropertyName("orientation")]
    public string Orientation { get; set; } = "horizontal";

    /// <summary>
    /// Whether selection is required
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    /// <summary>
    /// Default selected value
    /// </summary>
    [JsonPropertyName("default_value")]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Whether to show as buttons instead of radio buttons
    /// </summary>
    [JsonPropertyName("button_style")]
    public bool ButtonStyle { get; set; } = false;

    /// <summary>
    /// Spacing between options in pixels
    /// </summary>
    [JsonPropertyName("spacing")]
    public int Spacing { get; set; } = 10;
}

/// <summary>
/// Represents a single radio option
/// </summary>
public class RadioOption
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
}
