namespace Application.ApiContracts.FinanceContract.Responses;

public sealed class FinanceContractDetailResponse
{
    public Guid Id { get; set; }
    public Guid? SalesOrderId { get; set; }

    public string ContractNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Customer360Response? Customer360 { get; set; }
    public PartnerResponse? FinancialPartner { get; set; }
    public CreditPackageResponse? CreditPackage { get; set; }

    public DisbursementResponse? Disbursement { get; set; }

    public CavetResponse? Cavet { get; set; }

    public EvidenceResponse? Evidence { get; set; }
}

public sealed class Customer360Response
{
    public string FullName { get; set; } = string.Empty;
    public string Cccd { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public sealed class PartnerResponse
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? BankStaffName { get; set; }
}

public sealed class CreditPackageResponse
{
    public string? ContractNo { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public int? TermMonths { get; set; }
    public string? InterestRateRange { get; set; }
    public decimal? MonthlyPaymentAmount { get; set; }
}

public sealed class DisbursementResponse
{
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
}

public sealed class CavetResponse
{
    public string State { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public string? ReceiverName { get; set; }
    public string? StorageLocation { get; set; }
}

public sealed class EvidenceResponse
{
    public string? DisbursementTransferProofUrl { get; set; }
}

