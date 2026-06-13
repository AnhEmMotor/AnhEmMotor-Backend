namespace Application.ApiContracts.Statistical.Responses;

public class KpiCardsResponse
{
    public int InProgressCount { get; set; }
    public double AvgCompletionHours { get; set; }
    public decimal CumulativeRevenue { get; set; }
}
