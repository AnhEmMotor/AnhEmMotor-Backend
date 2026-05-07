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

    public Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Options.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<List<OptionEntity>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return GetQueryable(mode)
            .Where(o => o.Name != null && lowerNames.Contains(o.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public Task<List<OptionEntity>> GetAllWithOptionsAsync(CancellationToken cancellationToken)
    {
        return context.Options.Include(o => o.OptionValues).AsNoTracking().ToListAsync(cancellationToken);
    }
}
