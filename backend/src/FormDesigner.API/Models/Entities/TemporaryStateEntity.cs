using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormDesigner.API.Models.Entities;

/// <summary>
/// Entity representing temporary saved progress during form data entry.
/// Allows users to save their work and resume later without submitting.
/// </summary>
public class TemporaryStateEntity
{
    /// <summary>
    /// Unique identifier for this temporary state
    /// </summary>
    [Key]
    [Required]
    public Guid StateId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the form instance
    /// </summary>
    [Required]
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Navigation property to the form instance
    /// </summary>
    [ForeignKey(nameof(InstanceId))]
    public FormInstanceEntity? FormInstance { get; set; }

    /// <summary>
    /// Partial form data in JSON format (stored as JSONB in PostgreSQL)
    /// Contains incomplete field values, partial table data, etc.
    /// </summary>
    [Required]
    public string DataJson { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this state was saved
    /// </summary>
    [Required]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional user identifier who saved this state
    /// </summary>
    [MaxLength(256)]
    public string? UserId { get; set; }
}
