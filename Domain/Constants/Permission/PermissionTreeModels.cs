namespace Domain.Constants.Permission
{
    using System.Collections.Generic;

    public class PermissionModuleMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<PermissionFeatureMetadata> Features { get; set; } = new();
    }

    public class PermissionFeatureMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<PermissionActionMetadata> Permissions { get; set; } = new();
    }

    public class PermissionActionMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
