using Domain.Constants;
using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputReadRepository
{
    public IQueryable<InputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public IQueryable<InputEntity> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
