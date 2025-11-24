namespace Application.ApiContracts.File;

public class RestoreManyMediaFilesRequest
{
    public List<string> StoragePaths { get; set; } = [];
}
