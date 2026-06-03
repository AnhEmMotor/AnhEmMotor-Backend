namespace Application.ApiContracts.SupplierContracts.Requests;

public class CreateSupplierContractRequest
{
    public int? SupplierId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string? ContractFilePath { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal ContractValue { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Terms { get; set; }
    public string? Note { get; set; }

    public decimal? CreditLimit { get; set; }
    public int? PaymentWindowDays { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public int? MinimumVolumePerMonth { get; set; }
    public decimal? DiscountRate { get; set; }
    public Guid? ParentContractId { get; set; }
    public ICollection<SupplierContractItemDto>? ContractItems { get; set; }
}
