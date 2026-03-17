using Application.Interfaces.Repositories.Option;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OptionEntity = Domain.Entities.Option;

namespace Infrastructure.Repositories.Option;

public class OptionReadRepository(ApplicationDBContext context) : IOptionReadRepository
{
    public IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OptionEntity>(mode);
    }

    public async Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Options
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<OptionEntity>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetQueryable(mode)
            .Where(o => o.Name != null && names.Contains(o.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<OptionEntity>> GetAllWithOptionsAsync(CancellationToken cancellationToken)
    {
        return await context.Options
            .Include(o => o.OptionValues)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
