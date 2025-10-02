using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for HierarchicalChecklist widget - nested checklist with numbering.
/// Used for complex inspection checklists with hierarchical structures.
/// </summary>
public class HierarchicalChecklistSpec
{
    /// <summary>
    /// Root checklist items
    /// </summary>
    [JsonPropertyName("items")]
    public List<HierarchicalChecklistItem> Items { get; set; } = new();

    /// <summary>
    /// Numbering style (decimal, alpha, roman)
    /// </summary>
    [JsonPropertyName("numbering_style")]
    public string NumberingStyle { get; set; } = "decimal";

    /// <summary>
    /// Whether to show numbering
    /// </summary>
    [JsonPropertyName("show_numbering")]
    public bool ShowNumbering { get; set; } = true;

    /// <summary>
    /// Indentation size in pixels
    /// </summary>
    [JsonPropertyName("indent_size")]
    public int IndentSize { get; set; } = 20;

    /// <summary>
    /// Whether all items are required
    /// </summary>
    [JsonPropertyName("all_required")]
    public bool AllRequired { get; set; } = false;

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }
}

/// <summary>
/// Represents a single item in hierarchical checklist
/// </summary>
public class HierarchicalChecklistItem
{
    /// <summary>
    /// Item key/ID
    /// </summary>
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Item label/text
    /// </summary>
    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Item type (checkbox, text, select, radio)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkbox";

    /// <summary>
    /// Options for select/radio type
    /// </summary>
    [JsonPropertyName("options")]
    public List<string>? Options { get; set; }

    /// <summary>
    /// Whether this item is required
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    /// <summary>
    /// Nested child items
    /// </summary>
    [JsonPropertyName("children")]
    public List<HierarchicalChecklistItem>? Children { get; set; }

    /// <summary>
    /// Whether children are initially expanded
    /// </summary>
    [JsonPropertyName("expanded")]
    public bool Expanded { get; set; } = true;

    /// <summary>
    /// Custom numbering (overrides auto-numbering)
    /// </summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>
    /// Placeholder text for text inputs
    /// </summary>
    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Remarks/notes field
    /// </summary>
    [JsonPropertyName("remarks_field")]
    public bool RemarksField { get; set; } = false;
}
