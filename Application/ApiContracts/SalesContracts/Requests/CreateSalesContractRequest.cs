namespace Application.ApiContracts.SalesContracts.Requests;

public class CreateSalesContractRequest
{
    public int InvoiceId { get; set; }

    public string? SpecialTerms { get; set; }

    public string? WarrantyPeriod { get; set; }

    public string? WarrantyScope { get; set; }

    public string? Note { get; set; }
}
