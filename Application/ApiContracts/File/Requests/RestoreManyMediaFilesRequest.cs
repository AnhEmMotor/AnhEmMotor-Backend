using Application;
using Application.ApiContracts;
using Application.ApiContracts.File;

namespace Application.ApiContracts.File.Requests;

public class RestoreManyMediaFilesRequest
{
    public List<string> StoragePaths { get; set; } = [];
}
