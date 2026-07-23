
namespace Application.Interfaces.Repositories.Lead.Lead;

public interface ILeadDeleteRepository
{
    public Task ClearAllAsync(CancellationToken cancellationToken = default);

    public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
}
