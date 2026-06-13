namespace Application.ApiContracts.FinanceContract.Requests;

public class UploadDisbursementEvidenceRequest
{
    public Stream? FileContent { get; set; }
    public string? FileName { get; set; }
}

