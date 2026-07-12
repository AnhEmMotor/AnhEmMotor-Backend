namespace Application.ApiContracts.Statistical.Responses;

public class RevenueByCategoryResponse
{
    public string CategoryName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }

    public decimal Percentage { get; set; }
}

public class DailyCategoryRevenueResponse
{
    public string ReportDay { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }
}
