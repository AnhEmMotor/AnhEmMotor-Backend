using Domain.Entities;

namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherUsageRepository
{
    public Task<int> GetUserUsageCountAsync(int voucherId, Guid userId, CancellationToken cancellationToken = default);

    public Task<int> GetTotalUsageCountAsync(int voucherId, CancellationToken cancellationToken = default);

    public Task<OrderVoucher?> GetByVoucherAndOutputAsync(
        int voucherId,
        int outputId,
        CancellationToken cancellationToken = default);

    public Task AddAsync(OrderVoucher orderVoucher, CancellationToken cancellationToken = default);

    public Task<OrderVoucher?> GetByIdAsync(int orderVoucherId, CancellationToken cancellationToken = default);

    public void Remove(OrderVoucher orderVoucher);
}
