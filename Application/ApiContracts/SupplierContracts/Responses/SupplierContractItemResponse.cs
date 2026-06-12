namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractItemResponse
{
    public Guid Id { get; set; }
    public int ProductVariantId { get; set; }
    public string? SkuCode { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public decimal WholesalePrice { get; set; }
    public int? Moq { get; set; }
}
