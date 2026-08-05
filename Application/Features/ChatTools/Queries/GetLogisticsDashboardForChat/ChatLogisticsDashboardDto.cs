namespace Application.Features.ChatTools.Queries.GetLogisticsDashboardForChat;

public record ChatLogisticsDashboardDto
{
    public int FulfillmentWorkload { get; init; }

    public bool FulfillmentWorkloadIsOverload { get; init; }

    public decimal PendingUnreconciledCod { get; init; }

    public double OtifRate { get; init; }

    public double ReturnsClaimsRate { get; init; }

    public int ExceptionCount { get; init; }
}
