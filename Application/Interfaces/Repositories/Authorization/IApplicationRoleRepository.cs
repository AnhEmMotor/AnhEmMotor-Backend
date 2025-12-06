using Domain.Entities;

namespace Application.Interfaces.Repositories.Authorization;

public interface IApplicationRoleRepository
{
    Task<List<ApplicationRole>> GetRolesByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default);
}
