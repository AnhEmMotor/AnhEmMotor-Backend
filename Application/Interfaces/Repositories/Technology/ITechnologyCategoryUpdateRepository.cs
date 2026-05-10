using Domain.Entities;

namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyCategoryUpdateRepository
{
    public void Add(TechnologyCategory category);

    public void Update(TechnologyCategory category);

    public void Remove(TechnologyCategory category);
}
