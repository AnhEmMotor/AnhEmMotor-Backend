using Domain.Entities;

namespace Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;

public interface ITechnologyCategoryDeleteRepository
{
    public void Remove(Domain.Entities.TechnologyCategory category);
}
