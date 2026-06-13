namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractResponse
{
    public Guid Id { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string ContractNumber { get; set; } = string.Empty;

    public string? ContractFilePath { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public decimal ContractValue { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Terms { get; set; }

    public string? Note { get; set; }

    public decimal? CreditLimit { get; set; }

    public int? PaymentWindowDays { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? BankName { get; set; }

    public int? MinimumVolumePerMonth { get; set; }

    public decimal? DiscountRate { get; set; }

    public Guid? ParentContractId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
