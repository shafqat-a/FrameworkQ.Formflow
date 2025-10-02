using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Form metadata for organizational information.
/// </summary>
public class FormMetadata
{
    /// <summary>
    /// Organization name
    /// </summary>
    [JsonPropertyName("organization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Organization { get; set; }

    /// <summary>
    /// Document number
    /// </summary>
    [JsonPropertyName("document_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Effective date
    /// </summary>
    [JsonPropertyName("effective_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EffectiveDate { get; set; }

    /// <summary>
    /// Revision number
    /// </summary>
    [JsonPropertyName("revision")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Revision { get; set; }

    /// <summary>
    /// Reference links or IDs
    /// </summary>
    [JsonPropertyName("reference")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Reference { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Form options including print and permission settings.
/// </summary>
public class FormOptions
{
    /// <summary>
    /// Print settings
    /// </summary>
    [JsonPropertyName("print")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrintOptions? Print { get; set; }

    /// <summary>
    /// Permission settings
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PermissionOptions? Permissions { get; set; }
}

/// <summary>
/// Print settings for form output.
/// </summary>
public class PrintOptions
{
    /// <summary>
    /// Page size (e.g., "A4", "Letter")
    /// </summary>
    [JsonPropertyName("page_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PageSize { get; set; }

    /// <summary>
    /// Page orientation ("portrait" or "landscape")
    /// </summary>
    [JsonPropertyName("orientation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Orientation { get; set; }

    /// <summary>
    /// Page margins
    /// </summary>
    [JsonPropertyName("margins")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Margins? Margins { get; set; }
}

/// <summary>
/// Page margin settings.
/// </summary>
public class Margins
{
    /// <summary>
    /// Top margin (e.g., "1in", "2cm")
    /// </summary>
    [JsonPropertyName("top")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Top { get; set; }

    /// <summary>
    /// Right margin
    /// </summary>
    [JsonPropertyName("right")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Right { get; set; }

    /// <summary>
    /// Bottom margin
    /// </summary>
    [JsonPropertyName("bottom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Bottom { get; set; }

    /// <summary>
    /// Left margin
    /// </summary>
    [JsonPropertyName("left")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Left { get; set; }
}

/// <summary>
/// Permission options for form access control.
/// </summary>
public class PermissionOptions
{
    /// <summary>
    /// Roles allowed to view the form
    /// </summary>
    [JsonPropertyName("view")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? View { get; set; }

    /// <summary>
    /// Roles allowed to edit the form
    /// </summary>
    [JsonPropertyName("edit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Edit { get; set; }

    /// <summary>
    /// Roles allowed to delete the form
    /// </summary>
    [JsonPropertyName("delete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Delete { get; set; }
}

/// <summary>
/// Storage configuration for form data persistence.
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Storage mode (e.g., "database", "file")
    /// </summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mode { get; set; }

    /// <summary>
    /// Header fields to copy to detail tables
    /// </summary>
    [JsonPropertyName("copy_from_header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? CopyFromHeader { get; set; }

    /// <summary>
    /// Index specifications for performance
    /// </summary>
    [JsonPropertyName("indexes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object>>? Indexes { get; set; }
}
