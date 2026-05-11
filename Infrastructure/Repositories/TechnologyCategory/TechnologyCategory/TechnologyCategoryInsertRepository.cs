using Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.TechnologyCategory.TechnologyCategory;

public class TechnologyCategoryInsertRepository(ApplicationDBContext context) : ITechnologyCategoryInsertRepository
{
    public void Add(Domain.Entities.TechnologyCategory category)
    {
        context.TechnologyCategories.Add(category);
    }
}
