
namespace Application.Interfaces.Repositories.Lead.Lead;

public interface ILeadDeleteRepository
{
    public Task ClearAllAsync(CancellationToken cancellationToken = default);
}
