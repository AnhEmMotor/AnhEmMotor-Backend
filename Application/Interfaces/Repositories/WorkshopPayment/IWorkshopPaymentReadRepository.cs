using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.WorkshopPayment;

public interface IWorkshopPaymentReadRepository
{
    Task<PagedResult<Domain.Entities.WorkshopPayment>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken);

    Task<Domain.Entities.WorkshopPayment?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<object> GetStatsAsync(CancellationToken cancellationToken);
}
