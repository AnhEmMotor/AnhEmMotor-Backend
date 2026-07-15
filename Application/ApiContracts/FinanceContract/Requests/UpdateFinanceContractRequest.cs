namespace Application.ApiContracts.FinanceContract.Requests;

public class UpdateFinanceContractRequest
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int TermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string? DisbursementStatus { get; set; }
    public string? CavetLocation { get; set; }
    public DateTime? SignedDate { get; set; }
}
