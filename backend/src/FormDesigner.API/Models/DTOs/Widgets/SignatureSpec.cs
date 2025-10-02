using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Widgets;

/// <summary>
/// Specification for Signature widget - captures approval signatures.
/// Supports multiple signature roles (Reviewed by, Approved by, etc.)
/// </summary>
public class SignatureSpec
{
    /// <summary>
    /// Signature role/type (e.g., "Reviewed by (GMT-1)", "Approved by (DT)")
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Name field label (default: "Name")
    /// </summary>
    [JsonPropertyName("name_label")]
    public string NameLabel { get; set; } = "Name";

    /// <summary>
    /// Whether name field is required
    /// </summary>
    [JsonPropertyName("name_required")]
    public bool NameRequired { get; set; } = true;

    /// <summary>
    /// Designation field label (default: "Designation")
    /// </summary>
    [JsonPropertyName("designation_label")]
    public string DesignationLabel { get; set; } = "Designation";

    /// <summary>
    /// Whether designation field is shown
    /// </summary>
    [JsonPropertyName("show_designation")]
    public bool ShowDesignation { get; set; } = true;

    /// <summary>
    /// Date field label (default: "Date")
    /// </summary>
    [JsonPropertyName("date_label")]
    public string DateLabel { get; set; } = "Date";

    /// <summary>
    /// Whether date field is shown
    /// </summary>
    [JsonPropertyName("show_date")]
    public bool ShowDate { get; set; } = true;

    /// <summary>
    /// Whether to auto-fill date with current date
    /// </summary>
    [JsonPropertyName("auto_date")]
    public bool AutoDate { get; set; } = false;

    /// <summary>
    /// Whether signature image/drawing is required
    /// </summary>
    [JsonPropertyName("require_signature_image")]
    public bool RequireSignatureImage { get; set; } = false;

    /// <summary>
    /// Signature image type (draw, upload, both)
    /// </summary>
    [JsonPropertyName("signature_type")]
    public string SignatureType { get; set; } = "draw";

    /// <summary>
    /// Signature line width for drawing
    /// </summary>
    [JsonPropertyName("signature_width")]
    public int SignatureWidth { get; set; } = 400;

    /// <summary>
    /// Signature line height for drawing
    /// </summary>
    [JsonPropertyName("signature_height")]
    public int SignatureHeight { get; set; } = 100;

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    public string? CssClass { get; set; }
}
