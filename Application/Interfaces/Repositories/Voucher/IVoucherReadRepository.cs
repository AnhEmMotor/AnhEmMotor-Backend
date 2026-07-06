using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SieveModel = global::Sieve.Models.SieveModel;

namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherReadRepository
{
    Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Voucher, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.Voucher?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken);
}
