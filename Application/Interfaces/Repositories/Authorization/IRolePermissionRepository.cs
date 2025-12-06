using Domain.Entities;

namespace Application.Interfaces.Repositories.Authorization;

public interface IRolePermissionRepository
{
    Task AddRangeAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<RolePermission> rolePermissions);
    Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<List<RolePermission>> GetByPermissionIdsAsync(IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);
    Task<List<string>> GetPermissionNamesByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
    Task<List<string>> GetPermissionNamesByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
