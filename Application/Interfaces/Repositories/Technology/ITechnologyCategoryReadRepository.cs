using Domain.Entities;

namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyCategoryReadRepository
{
    public IQueryable<TechnologyCategory> GetQueryable();

    public Task<List<TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}
