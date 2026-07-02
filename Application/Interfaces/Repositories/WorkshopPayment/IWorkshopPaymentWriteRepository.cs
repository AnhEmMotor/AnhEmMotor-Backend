using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.WorkshopPayment;

public interface IWorkshopPaymentWriteRepository
{
    Task AddAsync(Domain.Entities.WorkshopPayment payment, CancellationToken cancellationToken);
}
