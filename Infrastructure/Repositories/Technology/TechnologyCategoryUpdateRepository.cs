using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Technology;

public class TechnologyCategoryUpdateRepository(ApplicationDBContext context) : ITechnologyCategoryUpdateRepository
{
    public void Add(TechnologyCategory category)
    {
        context.TechnologyCategories.Add(category);
    }

    public void Update(TechnologyCategory category)
    {
        context.TechnologyCategories.Update(category);
    }

    public void Remove(TechnologyCategory category)
    {
        context.TechnologyCategories.Remove(category);
    }
}
