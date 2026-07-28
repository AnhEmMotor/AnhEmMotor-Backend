using System.Text.Json.Serialization;

namespace Application.DTOs.Chat;

public record SteeringQueueItem(
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("mode")] string Mode);
