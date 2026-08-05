namespace Application.Features.ChatTools.Queries.GetWorkshopDashboardForChat;

public sealed record ChatWorkshopDashboardDto
{
    public int InProgressCount { get; init; }

    public double AvgCompletionHours { get; init; }

    public decimal CumulativeRevenue { get; init; }

    public int OverdueTicketsCount { get; init; }

    public int PartShortagesCount { get; init; }

    public int WarrantyRequestsCount { get; init; }

    public int ComplaintsCount { get; init; }
}
