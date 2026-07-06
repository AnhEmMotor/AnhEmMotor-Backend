using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using WorkshopPaymentEntity = Domain.Entities.WorkshopPayment;

namespace Application.Interfaces.Repositories;

public interface IWorkshopPaymentReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<WorkshopPaymentEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<WorkshopPaymentEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<WorkshopPaymentEntity?> GetByIdAsync(
        int id, CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<WorkshopPaymentEntity>> GetBySourceAsync(
        string sourceType, int sourceId, CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
