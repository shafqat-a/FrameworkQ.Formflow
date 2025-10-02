using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for Notes widget - displays instructions, notes, or warnings.
/// Used for inline form guidance and documentation.
/// </summary>
public class NotesSpec
{
    /// <summary>
    /// Note content/text
    /// </summary>
    [Required]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Note style/type (info, warning, note, instruction)
    /// </summary>
    [JsonPropertyName("style")]
    public string Style { get; set; } = "info";

    /// <summary>
    /// Note title/heading (optional)
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Whether content supports markdown formatting
    /// </summary>
    [JsonPropertyName("markdown")]
    public bool Markdown { get; set; } = false;

    /// <summary>
    /// Whether note is collapsible
    /// </summary>
    [JsonPropertyName("collapsible")]
    public bool Collapsible { get; set; } = false;

    /// <summary>
    /// Initial collapsed state (if collapsible)
    /// </summary>
    [JsonPropertyName("collapsed")]
    public bool Collapsed { get; set; } = false;

    /// <summary>
    /// Icon to display (optional)
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Background color (for custom styling)
    /// </summary>
    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Text color (for custom styling)
    /// </summary>
    [JsonPropertyName("text_color")]
    public string? TextColor { get; set; }
}
