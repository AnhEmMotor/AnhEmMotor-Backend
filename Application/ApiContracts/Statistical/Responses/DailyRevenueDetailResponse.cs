namespace Application.ApiContracts.Statistical.Responses;

public class DailyRevenueDetailResponse
{
    public string? ProductName { get; set; }
    public string? EmployeeName { get; set; }
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}
