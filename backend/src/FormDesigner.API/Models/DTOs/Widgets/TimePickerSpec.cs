using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for TimePicker widget - time selection field.
/// Supports 12h/24h formats with min/max constraints.
/// </summary>
public class TimePickerSpec
{
    /// <summary>
    /// Time format (12h, 24h)
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "24h";

    /// <summary>
    /// Minimum allowed time (HH:mm format)
    /// </summary>
    [JsonPropertyName("min_time")]
    public string? MinTime { get; set; }

    /// <summary>
    /// Maximum allowed time (HH:mm format)
    /// </summary>
    [JsonPropertyName("max_time")]
    public string? MaxTime { get; set; }

    /// <summary>
    /// Step minutes for time picker (e.g., 15, 30)
    /// </summary>
    [JsonPropertyName("step_minutes")]
    public int StepMinutes { get; set; } = 1;

    /// <summary>
    /// Default time value (HH:mm format)
    /// </summary>
    [JsonPropertyName("default_value")]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Whether field is required
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    /// <summary>
    /// Whether field is readonly
    /// </summary>
    [JsonPropertyName("readonly")]
    public bool Readonly { get; set; } = false;

    /// <summary>
    /// Placeholder text
    /// </summary>
    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Whether to show seconds
    /// </summary>
    [JsonPropertyName("show_seconds")]
    public bool ShowSeconds { get; set; } = false;

    /// <summary>
    /// Whether to use dropdown picker or manual input
    /// </summary>
    [JsonPropertyName("use_picker")]
    public bool UsePicker { get; set; } = true;

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Icon to display (optional)
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
