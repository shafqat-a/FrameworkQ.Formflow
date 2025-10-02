using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FormDesigner.API.Models.DTOs.Specs;
using FormDesigner.API.Models.DTOs.Widgets;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Represents a widget within a section.
/// Widgets are the base units of form interaction (field, table, grid, checklist, group).
/// </summary>
public class Widget
{
    /// <summary>
    /// Widget type: "field", "table", "grid", "checklist", "group"
    /// </summary>
    [Required(ErrorMessage = "Widget type is required")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Unique widget identifier. Must match pattern: ^[a-z0-9_-]+$
    /// </summary>
    [Required(ErrorMessage = "Widget ID is required")]
    [RegularExpression(@"^[a-z0-9_-]+$", ErrorMessage = "Widget ID must contain only lowercase letters, numbers, hyphens, and underscores")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Widget title (optional)
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    /// Multilingual labels for the widget
    /// </summary>
    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Conditional visibility expression
    /// </summary>
    [JsonPropertyName("when")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? When { get; set; }

    /// <summary>
    /// Help text for the widget
    /// </summary>
    [JsonPropertyName("help")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Help { get; set; }

    // Type-specific properties (only one should be populated based on Type)

    /// <summary>
    /// Field specification (when type = "field")
    /// </summary>
    [JsonPropertyName("field")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FieldSpec? Field { get; set; }

    /// <summary>
    /// Group fields (when type = "group")
    /// </summary>
    [JsonPropertyName("fields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FieldSpec>? Fields { get; set; }

    /// <summary>
    /// Group layout configuration (when type = "group")
    /// </summary>
    [JsonPropertyName("layout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Layout { get; set; }

    /// <summary>
    /// Table specification (when type = "table")
    /// </summary>
    [JsonPropertyName("table")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TableSpec? Table { get; set; }

    /// <summary>
    /// Grid specification (when type = "grid")
    /// </summary>
    [JsonPropertyName("grid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GridSpec? Grid { get; set; }

    /// <summary>
    /// Checklist specification (when type = "checklist")
    /// </summary>
    [JsonPropertyName("checklist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChecklistSpec? Checklist { get; set; }

    /// <summary>
    /// FormHeader specification (when type = "formheader")
    /// </summary>
    [JsonPropertyName("formheader")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FormHeaderSpec? FormHeader { get; set; }

    /// <summary>
    /// Signature specification (when type = "signature")
    /// </summary>
    [JsonPropertyName("signature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SignatureSpec? Signature { get; set; }

    /// <summary>
    /// Notes specification (when type = "notes")
    /// </summary>
    [JsonPropertyName("notes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NotesSpec? Notes { get; set; }

    /// <summary>
    /// HierarchicalChecklist specification (when type = "hierarchicalchecklist")
    /// </summary>
    [JsonPropertyName("hierarchicalchecklist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HierarchicalChecklistSpec? HierarchicalChecklist { get; set; }

    /// <summary>
    /// RadioGroup specification (when type = "radiogroup")
    /// </summary>
    [JsonPropertyName("radiogroup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RadioGroupSpec? RadioGroup { get; set; }

    /// <summary>
    /// CheckboxGroup specification (when type = "checkboxgroup")
    /// </summary>
    [JsonPropertyName("checkboxgroup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CheckboxGroupSpec? CheckboxGroup { get; set; }

    /// <summary>
    /// TimePicker specification (when type = "timepicker")
    /// </summary>
    [JsonPropertyName("timepicker")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimePickerSpec? TimePicker { get; set; }
}
