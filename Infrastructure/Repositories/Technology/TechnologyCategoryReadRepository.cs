using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Technology;

public class TechnologyCategoryReadRepository(ApplicationDBContext context) : ITechnologyCategoryReadRepository
{
    public IQueryable<TechnologyCategory> GetQueryable() => context.TechnologyCategories.AsQueryable();

    public Task<List<TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.TechnologyCategories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
