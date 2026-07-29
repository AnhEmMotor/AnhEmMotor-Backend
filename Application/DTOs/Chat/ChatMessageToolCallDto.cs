namespace Application.DTOs.Chat;

public record ChatMessageToolCallDto(string Name, string Label, string? Summary = null, string Status = "done");
