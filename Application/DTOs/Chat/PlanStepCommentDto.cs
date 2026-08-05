using System.Text.Json.Serialization;

namespace Application.DTOs.Chat;

public record PlanStepCommentDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt);
