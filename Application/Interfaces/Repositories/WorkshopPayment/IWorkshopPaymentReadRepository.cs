using Application.Common.Models;
using Domain.Primitives;
using Sieve.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.WorkshopPayment;

public interface IWorkshopPaymentReadRepository
{
    Task<PagedResult<Domain.Entities.WorkshopPayment>> GetPagedAsync(SieveModel sieveModel, CancellationToken cancellationToken);
    Task<Domain.Entities.WorkshopPayment?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
