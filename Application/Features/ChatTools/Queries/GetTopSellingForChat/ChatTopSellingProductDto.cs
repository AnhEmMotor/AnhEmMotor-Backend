namespace Application.Features.ChatTools.Queries.GetTopSellingForChat;

public record ChatTopSellingProductDto
{
    public string ProductName { get; init; } = string.Empty;

    public int UnitsSold { get; init; }

    public decimal Revenue { get; init; }

    public string Currency { get; init; } = "VND";
}
