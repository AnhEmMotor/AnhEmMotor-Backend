namespace Application.ApiContracts.FinanceContract.Responses;

public class FinanceContractDetailResponse
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








