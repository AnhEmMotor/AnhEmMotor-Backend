using Application.Interfaces.Repositories.Authorization;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Authorization;

public class ApplicationRoleRepository(ApplicationDBContext context) : IApplicationRoleRepository
{
    public Task<List<ApplicationRole>> GetRolesByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    { return context.Roles.Where(r => names.Contains(r.Name!)).ToListAsync(cancellationToken); }
}
