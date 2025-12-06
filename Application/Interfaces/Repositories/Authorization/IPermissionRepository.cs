using Domain.Entities;

namespace Application.Interfaces.Repositories.Authorization;

public interface IPermissionRepository
{
    Task<List<Permission>> GetPermissionsByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default);
}
