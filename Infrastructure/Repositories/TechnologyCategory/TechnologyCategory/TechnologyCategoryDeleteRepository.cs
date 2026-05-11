using Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.TechnologyCategory.TechnologyCategory;

public class TechnologyCategoryDeleteRepository(ApplicationDBContext context) : ITechnologyCategoryDeleteRepository
{
    public void Remove(Domain.Entities.TechnologyCategory category)
    {
        context.TechnologyCategories.Remove(category);
    }
}
