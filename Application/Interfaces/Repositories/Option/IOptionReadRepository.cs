using Domain.Constants;
using OptionEntity = Domain.Entities.Option;

namespace Application.Interfaces.Repositories.Option;

public interface IOptionReadRepository
{
    public IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<OptionEntity>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<OptionEntity>> GetAllWithOptionsAsync(CancellationToken cancellationToken);
}
