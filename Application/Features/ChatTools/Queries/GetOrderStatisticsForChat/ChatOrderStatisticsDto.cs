namespace Application.Features.ChatTools.Queries.GetOrderStatisticsForChat;

public record ChatOrderStatisticsDto
{
    public int TotalOrders { get; init; }

    public IReadOnlyDictionary<string, int> CountByStatus { get; init; } = new Dictionary<string, int>();
}
