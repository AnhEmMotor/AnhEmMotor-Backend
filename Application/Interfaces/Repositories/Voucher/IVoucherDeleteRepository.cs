using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherDeleteRepository
{
    void SoftDelete(Domain.Entities.Voucher voucher);
}
