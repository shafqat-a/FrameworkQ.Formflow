using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Represents a section within a page.
/// Sections organize related widgets visually.
/// </summary>
public class Section
{
    /// <summary>
    /// Unique section identifier within the page. Must match pattern: ^[a-z0-9_-]+$
    /// </summary>
    [Required(ErrorMessage = "Section ID is required")]
    [RegularExpression(@"^[a-z0-9_-]+$", ErrorMessage = "Section ID must contain only lowercase letters, numbers, hyphens, and underscores")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Section title displayed to users
    /// </summary>
    [Required(ErrorMessage = "Section title is required")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Multilingual labels for the section
    /// </summary>
    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Widgets within this section
    /// </summary>
    [Required(ErrorMessage = "Section must have widgets array")]
    [JsonPropertyName("widgets")]
    public List<Widget> Widgets { get; set; } = new();

    /// <summary>
    /// Parent section ID for hierarchical sections (null for root sections)
    /// </summary>
    [JsonPropertyName("parent_section_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParentSectionId { get; set; }

    /// <summary>
    /// Numbering style for hierarchical sections (auto, manual, none, decimal, alpha)
    /// </summary>
    [JsonPropertyName("numbering_style")]
    public string NumberingStyle { get; set; } = "auto";

    /// <summary>
    /// Section level (0 for root, 1+ for nested)
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; } = 0;

    /// <summary>
    /// Custom section number (overrides auto-numbering)
    /// </summary>
    [JsonPropertyName("number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Number { get; set; }

    /// <summary>
    /// Child sub-sections
    /// </summary>
    [JsonPropertyName("sections")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Section>? Sections { get; set; }

    /// <summary>
    /// Whether section is collapsible
    /// </summary>
    [JsonPropertyName("collapsible")]
    public bool Collapsible { get; set; } = false;

    /// <summary>
    /// Initial collapsed state (if collapsible)
    /// </summary>
    [JsonPropertyName("collapsed")]
    public bool Collapsed { get; set; } = false;
}
