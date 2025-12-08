using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Infrastructure.DBContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.Role
{
    public class RoleUpdateRepository(ApplicationDBContext context) : IRoleUpdateRepository
    {
        // AddRangeAsync chỉ thêm vào ChangeTracker, chưa lưu xuống DB
        public async Task AddPermissionsToRoleAsync(
            IEnumerable<RolePermission> rolePermissions,
            CancellationToken cancellationToken = default)
        {
            await context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken).ConfigureAwait(false);
        }

        // RemoveRange là đồng bộ, chỉ đánh dấu trạng thái Deleted trong ChangeTracker
        public void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions)
        {
            context.RolePermissions.RemoveRange(rolePermissions);
        }
    }
}
