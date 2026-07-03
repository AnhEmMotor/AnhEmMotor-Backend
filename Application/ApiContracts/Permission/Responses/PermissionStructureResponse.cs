using Domain.Constants.Permission;

namespace Application.ApiContracts.Permission.Responses;

public class PermissionStructureResponse
{
    public List<PermissionModuleMetadata> Modules { get; set; } = [];

    public Dictionary<string, List<string>> Conflicts { get; set; } = [];

    public Dictionary<string, List<string>> Dependencies { get; set; } = [];

    public List<PermissionMetadataResponse> Metadata { get; set; } = [];
}
