using Domain.Entities;

namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherUsageRepository
{
	Task<int> GetUserUsageCountAsync(int voucherId, Guid userId, CancellationToken cancellationToken = default);

	Task<int> GetTotalUsageCountAsync(int voucherId, CancellationToken cancellationToken = default);

	Task<OrderVoucher?> GetByVoucherAndOutputAsync(int voucherId, int outputId, CancellationToken cancellationToken = default);

	Task AddAsync(OrderVoucher orderVoucher, CancellationToken cancellationToken = default);

	Task<OrderVoucher?> GetByIdAsync(int orderVoucherId, CancellationToken cancellationToken = default);

	void Remove(OrderVoucher orderVoucher);
}
