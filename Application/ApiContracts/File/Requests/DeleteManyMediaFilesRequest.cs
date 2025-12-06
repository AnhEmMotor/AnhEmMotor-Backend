namespace Application.ApiContracts.File.Requests;

public class DeleteManyMediaFilesRequest
{
    public List<string> StoragePaths { get; set; } = [];
}
