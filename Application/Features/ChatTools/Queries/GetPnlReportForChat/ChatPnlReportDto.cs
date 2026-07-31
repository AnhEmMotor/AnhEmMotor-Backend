namespace Application.Features.ChatTools.Queries.GetPnlReportForChat;

public record ChatPnlReportDto
{
    public string Period { get; init; } = string.Empty;

    public decimal Revenue { get; init; }

    public decimal CostOfGoods { get; init; }

    public decimal GrossProfit { get; init; }

    public decimal Expenses { get; init; }

    public decimal NetProfit { get; init; }

    public string Currency { get; init; } = "VND";
}
