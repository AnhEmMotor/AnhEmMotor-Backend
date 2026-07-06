namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStatistics;

public class WorkshopPaymentStatisticsResponse
{
    public int Unpaid { get; set; }
    public decimal UnpaidAmount { get; set; }
    public int Partial { get; set; }
    public decimal PartialAmount { get; set; }
    public int PaidToday { get; set; }
    public decimal PaidTodayAmount { get; set; }
    public decimal MonthRevenue { get; set; }
}
