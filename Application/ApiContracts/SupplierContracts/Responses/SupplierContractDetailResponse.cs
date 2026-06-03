namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractDetailResponse : SupplierContractResponse
{
    public string? SupplierCode { get; set; }
    public string? SupplierContactName { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierEmail { get; set; }
    public string? SupplierAddress { get; set; }

    public ICollection<SupplierContractItemResponse> SkuPriceList { get; set; } = [];
    public ICollection<SupplierContractAuditLogResponse> AuditLogs { get; set; } = [];
}
