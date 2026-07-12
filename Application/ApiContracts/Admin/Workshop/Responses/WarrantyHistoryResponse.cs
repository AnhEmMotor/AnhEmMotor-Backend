namespace Application.ApiContracts.Admin.Workshop.Responses;

public class WarrantyHistoryResponse
{
    public int Id { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;

    public int Status { get; set; }

    public string? StatusText { get; set; }

    public string IssueDescription { get; set; } = string.Empty;

    public string? ManufacturerDecision { get; set; }

    public bool IsRecall { get; set; }

    public decimal TotalPartsCost { get; set; }

    public decimal TotalLaborCost { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<WarrantyClaimPartResponse> Parts { get; set; } = new();
}
