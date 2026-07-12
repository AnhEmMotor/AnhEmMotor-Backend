using Application.Interfaces.Repositories.Voucher;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Voucher;

public class VoucherUpdateRepository(ApplicationDBContext context) : IVoucherUpdateRepository
{
    public async Task AddAsync(Domain.Entities.Voucher voucher, CancellationToken cancellationToken)
    {
        await context.Vouchers.AddAsync(voucher, cancellationToken);
    }

    public void Update(Domain.Entities.Voucher voucher)
    {
        context.Vouchers.Update(voucher);
    }
}
