using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a checklist widget.
/// </summary>
public class ChecklistSpec
{
    /// <summary>
    /// Checklist items
    /// </summary>
    [Required(ErrorMessage = "Checklist must have items")]
    [MinLength(1, ErrorMessage = "Checklist must have at least one item")]
    [JsonPropertyName("items")]
    public List<ChecklistItem> Items { get; set; } = new();
}

/// <summary>
/// Individual item within a checklist.
/// </summary>
public class ChecklistItem
{
    /// <summary>
    /// Unique key for the checklist item
    /// </summary>
    [Required(ErrorMessage = "Checklist item key is required")]
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Label displayed for the checklist item
    /// </summary>
    [Required(ErrorMessage = "Checklist item label is required")]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Item type (e.g., "checkbox", "radio")
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}
