using Domain.Entities;

namespace Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;

public interface ITechnologyCategoryUpdateRepository
{
    public void Update(Domain.Entities.TechnologyCategory category);
    public void Restore(Domain.Entities.TechnologyCategory category);
}
