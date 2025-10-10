using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a table widget with repeating rows.
/// </summary>
public class TableSpec
{
    /// <summary>
    /// Row mode: "finite" (fixed number) or "infinite" (unlimited)
    /// </summary>
    [JsonPropertyName("row_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RowMode { get; set; }

    /// <summary>
    /// Minimum number of rows (for finite mode)
    /// </summary>
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Min { get; set; }

    /// <summary>
    /// Maximum number of rows (for finite mode)
    /// </summary>
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Max { get; set; }

    /// <summary>
    /// Row key field name for unique identification
    /// </summary>
    [JsonPropertyName("row_key")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RowKey { get; set; }

    /// <summary>
    /// Table column definitions
    /// </summary>
    [Required(ErrorMessage = "Table must have columns")]
    [MinLength(1, ErrorMessage = "Table must have at least one column")]
    [JsonPropertyName("columns")]
    public List<ColumnSpec> Columns { get; set; } = new();

    /// <summary>
    /// Row generators for dynamic row creation
    /// </summary>
    [JsonPropertyName("row_generators")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<RowGenerator>? RowGenerators { get; set; }

    /// <summary>
    /// Aggregate calculations (sum, avg, etc.)
    /// </summary>
    [JsonPropertyName("aggregates")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object>>? Aggregates { get; set; }

    /// <summary>
    /// Multi-row header configuration for complex table structures
    /// </summary>
    [JsonPropertyName("multi_row_headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TableHeaderRow>? MultiRowHeaders { get; set; }

    /// <summary>
    /// Merged cells configuration (colspan/rowspan)
    /// </summary>
    [JsonPropertyName("merged_cells")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<MergedCell>? MergedCells { get; set; }

    /// <summary>
    /// Checkbox column indices
    /// </summary>
    [JsonPropertyName("checkbox_columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? CheckboxColumns { get; set; }

    /// <summary>
    /// Radio column indices
    /// </summary>
    [JsonPropertyName("radio_columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? RadioColumns { get; set; }

    /// <summary>
    /// Whether table allows row addition by user
    /// </summary>
    [JsonPropertyName("allow_add_rows")]
    public bool AllowAddRows { get; set; } = true;

    /// <summary>
    /// Whether table allows row deletion by user
    /// </summary>
    [JsonPropertyName("allow_delete_rows")]
    public bool AllowDeleteRows { get; set; } = true;

    /// <summary>
    /// Initial rows data to pre-populate the table (array of objects with column values)
    /// Example: [{"time": "7:00", "value": "10"}, {"time": "8:00", "value": "20"}]
    /// </summary>
    [JsonPropertyName("initial_rows")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object>>? InitialRows { get; set; }
}

/// <summary>
/// Row generator specification for creating rows dynamically.
/// </summary>
public class RowGenerator
{
    /// <summary>
    /// Generator type: "range" or "values"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Source data type: "integer", "date", "time", "datetime"
    /// </summary>
    [JsonPropertyName("from_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FromType { get; set; }

    /// <summary>
    /// Start value for range
    /// </summary>
    [JsonPropertyName("from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? From { get; set; }

    /// <summary>
    /// End value for range
    /// </summary>
    [JsonPropertyName("to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? To { get; set; }

    /// <summary>
    /// Step/increment value
    /// </summary>
    [JsonPropertyName("step")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Step { get; set; }

    /// <summary>
    /// List of values (for type = "values")
    /// </summary>
    [JsonPropertyName("values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Values { get; set; }
}

/// <summary>
/// Multi-row header specification for complex table structures.
/// </summary>
public class TableHeaderRow
{
    /// <summary>
    /// Header cells in this row
    /// </summary>
    [Required]
    [JsonPropertyName("cells")]
    public List<TableHeaderCell> Cells { get; set; } = new();
}

/// <summary>
/// Individual header cell with span capabilities.
/// </summary>
public class TableHeaderCell
{
    /// <summary>
    /// Cell label/text
    /// </summary>
    [Required]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Column span (default 1)
    /// </summary>
    [JsonPropertyName("colspan")]
    public int Colspan { get; set; } = 1;

    /// <summary>
    /// Row span (default 1)
    /// </summary>
    [JsonPropertyName("rowspan")]
    public int Rowspan { get; set; } = 1;

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    [JsonPropertyName("css_class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CssClass { get; set; }
}

/// <summary>
/// Merged cell specification for table body cells.
/// </summary>
public class MergedCell
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
    /// Row span
    /// </summary>
    [JsonPropertyName("rowspan")]
    public int Rowspan { get; set; } = 1;

    /// <summary>
    /// Column span
    /// </summary>
    [JsonPropertyName("colspan")]
    public int Colspan { get; set; } = 1;
}
