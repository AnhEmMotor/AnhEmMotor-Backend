namespace Application.ApiContracts.Brand.Responses;

public class ImportBrandsResult
{
    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public string? ErrorFileUrl { get; set; }

    public string? ErrorFileWithReasonUrl { get; set; }
}
