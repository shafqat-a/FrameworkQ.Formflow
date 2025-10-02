using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs.Specs;

/// <summary>
/// Specification for a grid widget (two-dimensional data entry).
/// </summary>
public class GridSpec
{
    /// <summary>
    /// Row specification
    /// </summary>
    [JsonPropertyName("rows")]
    public GridRowSpec? Rows { get; set; }

    /// <summary>
    /// Column specification
    /// </summary>
    [JsonPropertyName("columns")]
    public GridColumnSpec? Columns { get; set; }

    /// <summary>
    /// Cell specification (defines what type of data goes in each cell)
    /// </summary>
    [JsonPropertyName("cell")]
    public GridCellSpec? Cell { get; set; }
}

/// <summary>
/// Row specification for grid.
/// </summary>
public class GridRowSpec
{
    /// <summary>
    /// Row generator type: "range" or "values"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Source data type for range: "integer", "date", "time", "datetime"
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
/// Column specification for grid.
/// </summary>
public class GridColumnSpec
{
    /// <summary>
    /// Column generator type: "range" or "values"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Source data type for range: "integer", "date", "time", "datetime"
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
/// Cell specification for grid (defines data type of cell values).
/// </summary>
public class GridCellSpec
{
    /// <summary>
    /// Cell data type: string, integer, decimal, bool
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether cells are required
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>
    /// Minimum value (for numeric types)
    /// </summary>
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Min { get; set; }

    /// <summary>
    /// Maximum value (for numeric types)
    /// </summary>
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Max { get; set; }
}
