using Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.TechnologyCategory.TechnologyCategory;

public class TechnologyCategoryUpdateRepository(ApplicationDBContext context) : ITechnologyCategoryUpdateRepository
{
    public void Update(Domain.Entities.TechnologyCategory category)
    {
        context.TechnologyCategories.Update(category);
    }

    public void Restore(Domain.Entities.TechnologyCategory category)
    {
        category.DeletedAt = null;
        context.TechnologyCategories.Update(category);
    }
}
