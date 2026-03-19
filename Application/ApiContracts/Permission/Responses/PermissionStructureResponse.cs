
namespace Application.ApiContracts.Permission.Responses;

public class PermissionStructureResponse
{
    public Dictionary<string, List<string>> Groups { get; set; } = [];

    public Dictionary<string, List<string>> Conflicts { get; set; } = [];

    public Dictionary<string, List<string>> Dependencies { get; set; } = [];

    public List<PermissionMetadataResponse> Metadata { get; set; } = [];
}
