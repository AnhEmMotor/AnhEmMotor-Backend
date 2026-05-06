using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Technology;

public class TechnologyCategoryRepository(ApplicationDBContext context) : ITechnologyCategoryRepository
{
    public IQueryable<TechnologyCategory> GetQueryable() => context.TechnologyCategories.AsQueryable();

    public void Add(TechnologyCategory category)
    {
        context.TechnologyCategories.Add(category);
    }
}
