namespace Domain.Constants.Permission
{
    public class PermissionMetadata(string displayName, string description)
    {
        public string DisplayName { get; } = displayName;

        public string Description { get; } = description;
    }
}
