namespace Application.ApiContracts.File.Requests;

public class RestoreManyMediaFilesRequest
{
    public List<string> StoragePaths { get; set; } = [];
}
