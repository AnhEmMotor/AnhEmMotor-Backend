namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractStatisticsResponse
{
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int PendingApproval { get; set; }
    public int ExpiredContracts { get; set; }
    public int ExpiringContracts { get; set; }
}
