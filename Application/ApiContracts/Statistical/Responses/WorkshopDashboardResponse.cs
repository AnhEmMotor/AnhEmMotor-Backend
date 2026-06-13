namespace Application.ApiContracts.Statistical.Responses;

public class WorkshopDashboardResponse
{
    public KpiCardsResponse KpiCards { get; set; } = new();
    public UrgentAlertsResponse Alerts { get; set; } = new();
    public AnalyticsResponse Analytics { get; set; } = new();
    public ProductivityResponse Productivity { get; set; } = new();
}
