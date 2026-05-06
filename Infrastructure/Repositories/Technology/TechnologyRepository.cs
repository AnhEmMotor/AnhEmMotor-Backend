using Application.Interfaces.Repositories.Technology;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Technology;

public class TechnologyRepository(ApplicationDBContext context) : ITechnologyRepository
{
    public IQueryable<Domain.Entities.Technology> GetQueryable() => context.Technologies.AsQueryable();

    public Task<Domain.Entities.Technology?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Technologies
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public void Add(Domain.Entities.Technology technology)
    {
        context.Technologies.Add(technology);
    }
}
