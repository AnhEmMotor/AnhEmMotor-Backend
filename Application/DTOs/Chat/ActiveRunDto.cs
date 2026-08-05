namespace Application.DTOs.Chat;

public record ActiveRunDto(
    Guid RunId,
    string Status,
    long LastSeq,
    DateTime? StartedAt,
    string UserMessage,
    string PartialOutput);
