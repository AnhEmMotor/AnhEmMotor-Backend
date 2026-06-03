namespace Application.ApiContracts.FinanceContract.Requests;

public sealed class UploadDisbursementEvidenceRequest
{
    public Stream? FileContent { get; set; }
    public string? FileName { get; set; }
}

