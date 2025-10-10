using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a group widget (multiple fields arranged in a layout)
/// </summary>
public class GroupSpec
{
    /// <summary>
    /// Fields contained in this group
    /// </summary>
    [JsonPropertyName("fields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FieldSpec>? Fields { get; set; }

    /// <summary>
    /// Layout configuration for field arrangement
    /// </summary>
    [JsonPropertyName("layout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GroupLayout? Layout { get; set; }

    /// <summary>
    /// Cell-based layout for precise positioning (alternative to auto-flow)
    /// </summary>
    [JsonPropertyName("cells")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FieldCell>? Cells { get; set; }
}

/// <summary>
/// Layout configuration for group widget
/// </summary>
public class GroupLayout
{
    /// <summary>
    /// Layout style: "inline" (vertical stack), "table" (bordered grid), "grid" (CSS grid)
    /// </summary>
    [JsonPropertyName("style")]
    public string Style { get; set; } = "inline";

    /// <summary>
    /// Number of columns for table/grid layouts
    /// </summary>
    [JsonPropertyName("columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Columns { get; set; }

    /// <summary>
    /// Number of rows for table layout (optional, can be auto-calculated)
    /// </summary>
    [JsonPropertyName("rows")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Rows { get; set; }

    /// <summary>
    /// Whether to show borders (for table style)
    /// </summary>
    [JsonPropertyName("bordered")]
    public bool Bordered { get; set; } = true;

    /// <summary>
    /// Compact mode (reduced padding)
    /// </summary>
    [JsonPropertyName("compact")]
    public bool Compact { get; set; } = false;

    /// <summary>
    /// Custom CSS classes to apply
    /// </summary>
    [JsonPropertyName("css_class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CssClass { get; set; }
}

/// <summary>
/// Represents a field positioned in a specific table cell with span capabilities
/// </summary>
public class FieldCell
{
    /// <summary>
    /// Row index (0-based)
    /// </summary>
    [Required]
    [JsonPropertyName("row")]
    public int Row { get; set; }

    /// <summary>
    /// Column index (0-based)
    /// </summary>
    [Required]
    [JsonPropertyName("col")]
    public int Col { get; set; }

    /// <summary>
    /// Row span (default 1)
    /// </summary>
    [JsonPropertyName("rowspan")]
    public int Rowspan { get; set; } = 1;

    /// <summary>
    /// Column span (default 1)
    /// </summary>
    [JsonPropertyName("colspan")]
    public int Colspan { get; set; } = 1;

    /// <summary>
    /// Field specification for this cell
    /// </summary>
    [Required]
    [JsonPropertyName("field")]
    public FieldSpec Field { get; set; } = new();

    /// <summary>
    /// Cell alignment: "left", "center", "right"
    /// </summary>
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Align { get; set; }

    /// <summary>
    /// Cell vertical alignment: "top", "middle", "bottom"
    /// </summary>
    [JsonPropertyName("valign")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Valign { get; set; }

    /// <summary>
    /// Custom CSS class for this cell
    /// </summary>
    [JsonPropertyName("css_class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CssClass { get; set; }
}
