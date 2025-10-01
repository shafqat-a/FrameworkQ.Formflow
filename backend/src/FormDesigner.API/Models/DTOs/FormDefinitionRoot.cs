using System.Text.Json.Serialization;

namespace FormDesigner.API.Models.DTOs;

/// <summary>
/// Root DTO for form definition API requests/responses.
/// Wraps the FormDefinition in a "form" property to match DSL v0.1 structure.
/// </summary>
public class FormDefinitionRoot
{
    /// <summary>
    /// The complete form definition
    /// </summary>
    [JsonPropertyName("form")]
    public FormDefinition Form { get; set; } = new();
}
