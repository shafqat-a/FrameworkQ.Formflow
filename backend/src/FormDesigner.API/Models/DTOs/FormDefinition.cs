using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Complete form definition matching DSL v0.1 specification.
/// All JSON property names use snake_case as per DSL requirements.
/// </summary>
public class FormDefinition
{
    /// <summary>
    /// Unique form identifier. Must match pattern: ^[a-z0-9_-]+$
    /// </summary>
    [Required(ErrorMessage = "Form ID is required")]
    [RegularExpression(@"^[a-z0-9_-]+$", ErrorMessage = "Form ID must contain only lowercase letters, numbers, hyphens, and underscores")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Form title displayed to users
    /// </summary>
    [Required(ErrorMessage = "Form title is required")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Form version (e.g., "1.0", "2.1")
    /// </summary>
    [Required(ErrorMessage = "Form version is required")]
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Supported locales (e.g., ["en", "bn"])
    /// </summary>
    [JsonPropertyName("locale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Locale { get; set; }

    /// <summary>
    /// Multilingual labels for the form
    /// </summary>
    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Form metadata (organization, document number, tags, etc.)
    /// </summary>
    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FormMetadata? Meta { get; set; }

    /// <summary>
    /// Form options (print settings, permissions)
    /// </summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FormOptions? Options { get; set; }

    /// <summary>
    /// Storage configuration for form data
    /// </summary>
    [JsonPropertyName("storage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StorageOptions? Storage { get; set; }

    /// <summary>
    /// Form pages (at least one required)
    /// </summary>
    [Required(ErrorMessage = "Form must have at least one page")]
    [MinLength(1, ErrorMessage = "Form must have at least one page")]
    [JsonPropertyName("pages")]
    public List<Page> Pages { get; set; } = new();
}
