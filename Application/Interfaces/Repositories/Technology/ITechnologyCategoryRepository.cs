using Domain.Entities;

namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyCategoryRepository
{
    public IQueryable<TechnologyCategory> GetQueryable();
    public void Add(TechnologyCategory category);
}
