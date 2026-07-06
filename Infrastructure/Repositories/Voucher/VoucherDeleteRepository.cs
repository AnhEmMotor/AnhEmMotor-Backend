using Application.Interfaces.Repositories.Voucher;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Voucher;

public class VoucherDeleteRepository(ApplicationDBContext context) : IVoucherDeleteRepository
{
    public void SoftDelete(Domain.Entities.Voucher voucher)
    {
        context.SoftDeleteUsingSetColumn(voucher);
    }
}
