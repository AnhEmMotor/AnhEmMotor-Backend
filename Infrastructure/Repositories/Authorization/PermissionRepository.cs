using Application.Interfaces.Repositories.Authorization;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Authorization;

public class PermissionRepository(ApplicationDBContext context) : IPermissionRepository
{
    public Task<List<Permission>> GetPermissionsByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    { return context.Permissions.Where(p => names.Contains(p.Name)).ToListAsync(cancellationToken); }
}
