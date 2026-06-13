namespace Application.ApiContracts.FinanceContract.Requests;

public class UpdateDisbursementPaymentRequest
{
    public decimal ActualAmount { get; set; }
    public DateTime? ActualDate { get; set; }
}

