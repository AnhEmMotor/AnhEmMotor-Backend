using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherUpdateRepository
{
    Task AddAsync(Domain.Entities.Voucher voucher, CancellationToken cancellationToken);
    void Update(Domain.Entities.Voucher voucher);
}
