using Application.Interfaces.Repositories.Technology;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology;

public class TechnologyReadRepository(ApplicationDBContext context) : ITechnologyReadRepository
{
    public Task<IEnumerable<TechnologyEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<TechnologyEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<TechnologyEntity>>(t => t.Result, cancellationToken);
    }

    public Task<TechnologyEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<TechnologyEntity>(mode)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public IQueryable<TechnologyEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<TechnologyEntity>(mode);
    }
}
