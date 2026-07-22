using Application.Interfaces.Repositories.Voucher;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Voucher;

public class VoucherUsageRepository(ApplicationDBContext context) : IVoucherUsageRepository
{
    public Task<int> GetUserUsageCountAsync(int voucherId, Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Set<OrderVoucher>()
            .Where(ov => ov.VoucherId == voucherId && ov.Output != null && ov.Output.BuyerId == userId)
            .CountAsync(cancellationToken);
    }

    public Task<int> GetTotalUsageCountAsync(int voucherId, CancellationToken cancellationToken = default)
    {
        return context.Set<OrderVoucher>().Where(ov => ov.VoucherId == voucherId).CountAsync(cancellationToken);
    }

    public Task<OrderVoucher?> GetByVoucherAndOutputAsync(
        int voucherId,
        int outputId,
        CancellationToken cancellationToken = default)
    {
        return context.Set<OrderVoucher>()
            .FirstOrDefaultAsync(ov => ov.VoucherId == voucherId && ov.OutputId == outputId, cancellationToken);
    }

    public Task<OrderVoucher?> GetByIdAsync(int orderVoucherId, CancellationToken cancellationToken = default)
    {
        return context.Set<OrderVoucher>().FirstOrDefaultAsync(ov => ov.Id == orderVoucherId, cancellationToken);
    }

    public async Task AddAsync(OrderVoucher orderVoucher, CancellationToken cancellationToken = default)
    {
        await context.Set<OrderVoucher>().AddAsync(orderVoucher, cancellationToken);
    }

    public void Remove(OrderVoucher orderVoucher)
    {
        context.Set<OrderVoucher>().Remove(orderVoucher);
    }
}
