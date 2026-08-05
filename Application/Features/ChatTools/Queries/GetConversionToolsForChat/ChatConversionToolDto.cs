namespace Application.Features.ChatTools.Queries.GetConversionToolsForChat;

public record ChatConversionToolDto
{
    public int Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public int Views { get; init; }

    public int Clicks { get; init; }

    public int Leads { get; init; }
}
