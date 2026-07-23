using Application.ApiContracts.Admin.Warranty;
using Domain.Constants;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermReadRepository
{
    public Task<global::Domain.Primitives.PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<global::Domain.Entities.WarrantyTerm, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<global::Domain.Entities.WarrantyTerm>> GetAllAsync(
        CancellationToken cancellationToken,
        Func<IQueryable<global::Domain.Entities.WarrantyTerm>, IQueryable<global::Domain.Entities.WarrantyTerm>>? include = null,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<global::Domain.Entities.WarrantyTerm?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        Func<IQueryable<global::Domain.Entities.WarrantyTerm>, IQueryable<global::Domain.Entities.WarrantyTerm>>? include = null,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<WarrantyTermStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken);
}
