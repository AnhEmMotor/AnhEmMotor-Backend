namespace Application.ApiContracts.SalesContracts.Requests;

public class UpdateSalesContractRequest
{
  public string? SpecialTerms { get; set; }
  public string? WarrantyPeriod { get; set; }
  public string? WarrantyScope { get; set; }
  public string? Note { get; set; }
}
