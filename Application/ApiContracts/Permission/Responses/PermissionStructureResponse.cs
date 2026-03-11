namespace Application.ApiContracts.Permission.Responses;

public class PermissionMetadataResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PermissionStructureResponse
{
    public Dictionary<string, List<string>> Groups { get; set; } = [];
    public Dictionary<string, List<string>> Conflicts { get; set; } = [];
    public Dictionary<string, List<string>> Dependencies { get; set; } = [];
    public List<PermissionMetadataResponse> Metadata { get; set; } = [];
}
