namespace Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }

    public ApplicationRole? Role { get; set; }

    public int PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
