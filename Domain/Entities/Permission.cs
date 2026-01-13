namespace Domain.Entities;

public class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
