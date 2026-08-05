namespace Application.DTOs.Chat;

public record ChatRunEventsResult(IReadOnlyList<ChatRunEventDto> Events, bool RunIsTerminal);
