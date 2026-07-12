namespace Application.ApiContracts.Admin.Workshop.Responses;

public class WarrantyClaimPartResponse
{
    public int Id { get; set; }

    public int WarrantyClaimId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string PartCode { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Status { get; set; }

    public string? StatusText { get; set; }
}
