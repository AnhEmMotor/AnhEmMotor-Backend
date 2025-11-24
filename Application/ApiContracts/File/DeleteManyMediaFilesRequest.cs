namespace Application.ApiContracts.File;

public class DeleteManyMediaFilesRequest
{
    public List<string> StoragePaths { get; set; } = [];
}
