namespace Application.Features.SalesContracts.Queries.GetSalesContractStatistics;

public class SalesContractStatisticsResponse
{
  public int DraftCount { get; set; }
  public int SignedCount { get; set; }
  public int FulfilledCount { get; set; }
  public int OverdueCount { get; set; }
}
