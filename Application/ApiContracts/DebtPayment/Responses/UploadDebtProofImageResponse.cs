namespace Application.ApiContracts.DebtPayment.Responses;

public class UploadDebtProofImageResponse
{
    public int MediaFileId { get; set; }

    public string Url { get; set; } = null!;
}
