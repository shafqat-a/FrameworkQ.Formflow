using System.ComponentModel.DataAnnotations;

namespace FormDesigner.API.Models.Entities;

/// <summary>
/// Entity representing a form definition stored in the database.
/// Supports both draft (editable) and committed (locked for runtime) states.
/// </summary>
public class FormDefinitionEntity
{
    /// <summary>
    /// Unique identifier for the form. Must match pattern: ^[a-z0-9_-]+$
    /// </summary>
    [Key]
    [Required]
    [RegularExpression(@"^[a-z0-9_-]+$", ErrorMessage = "Form ID must contain only lowercase letters, numbers, hyphens, and underscores")]
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Form version string (e.g., "1.0", "2.1")
    /// </summary>
    [Required]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Complete form definition in JSON format (stored as JSONB in PostgreSQL)
    /// Contains the entire DSL structure including pages, sections, widgets
    /// </summary>
    [Required]
    public string DslJson { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this form has been committed and is available for runtime execution.
    /// Draft forms (IsCommitted=false) can be edited.
    /// Committed forms (IsCommitted=true) are locked and available for data entry.
    /// </summary>
    [Required]
    public bool IsCommitted { get; set; } = false;

    /// <summary>
    /// Timestamp when the form was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the form was last updated (null if never updated)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag. When false, form is considered deleted but remains in database.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;
}
