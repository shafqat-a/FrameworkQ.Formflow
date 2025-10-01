using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormDesigner.API.Models.Entities;

/// <summary>
/// Entity representing a runtime form instance where users enter data.
/// Each instance is linked to a committed form definition.
/// </summary>
public class FormInstanceEntity
{
    /// <summary>
    /// Unique identifier for this form instance
    /// </summary>
    [Key]
    [Required]
    public Guid InstanceId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the committed form definition
    /// </summary>
    [Required]
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the form definition
    /// </summary>
    [ForeignKey(nameof(FormId))]
    public FormDefinitionEntity? FormDefinition { get; set; }

    /// <summary>
    /// Current status of this instance: "draft" or "submitted"
    /// Draft = data entry in progress (temporary state)
    /// Submitted = data persisted to database tables
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Form data in JSON format (stored as JSONB in PostgreSQL)
    /// Contains field values, table row data, etc.
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// Timestamp when the instance was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the instance was submitted (null if still draft)
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Optional user identifier who created/owns this instance
    /// </summary>
    [MaxLength(256)]
    public string? UserId { get; set; }
}
