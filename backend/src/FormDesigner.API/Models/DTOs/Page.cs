using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Represents a page within a form.
/// Forms can have multiple pages for complex workflows.
/// </summary>
public class Page
{
    /// <summary>
    /// Unique page identifier within the form. Must match pattern: ^[a-z0-9_-]+$
    /// </summary>
    [Required(ErrorMessage = "Page ID is required")]
    [RegularExpression(@"^[a-z0-9_-]+$", ErrorMessage = "Page ID must contain only lowercase letters, numbers, hyphens, and underscores")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Page title displayed to users
    /// </summary>
    [Required(ErrorMessage = "Page title is required")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Multilingual labels for the page
    /// </summary>
    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Sections within this page (at least one typically required)
    /// </summary>
    [Required(ErrorMessage = "Page must have sections array")]
    [JsonPropertyName("sections")]
    public List<Section> Sections { get; set; } = new();
}
