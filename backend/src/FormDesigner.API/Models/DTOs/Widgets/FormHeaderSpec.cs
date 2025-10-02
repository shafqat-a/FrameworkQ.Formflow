using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for FormHeader widget - displays document metadata.
/// Used in enterprise forms to show document information, revision details, and page numbers.
/// </summary>
public class FormHeaderSpec
{
    /// <summary>
    /// Document number (e.g., "QF-GMD-06")
    /// </summary>
    [JsonPropertyName("document_no")]
    public string? DocumentNo { get; set; }

    /// <summary>
    /// Revision number (e.g., "01", "02")
    /// </summary>
    [JsonPropertyName("revision_no")]
    public string? RevisionNo { get; set; }

    /// <summary>
    /// Effective date of the form
    /// </summary>
    [JsonPropertyName("effective_date")]
    public string? EffectiveDate { get; set; }

    /// <summary>
    /// Page number display (e.g., "1 of 2")
    /// </summary>
    [JsonPropertyName("page_number")]
    public string? PageNumber { get; set; }

    /// <summary>
    /// Organization name (e.g., "POWER GRID COMPANY OF BANGLADESH LTD.")
    /// </summary>
    [JsonPropertyName("organization")]
    public string? Organization { get; set; }

    /// <summary>
    /// Form title (e.g., "Sub-Station Performance Report")
    /// </summary>
    [JsonPropertyName("form_title")]
    public string? FormTitle { get; set; }

    /// <summary>
    /// Form category/type (e.g., "QUALITY FORMS")
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Whether to show the header on all pages
    /// </summary>
    [JsonPropertyName("show_on_all_pages")]
    public bool ShowOnAllPages { get; set; } = true;

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }
}
