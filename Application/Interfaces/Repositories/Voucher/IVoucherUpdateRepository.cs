
namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherUpdateRepository
{
    public Task AddAsync(Domain.Entities.Voucher voucher, CancellationToken cancellationToken);

    public void Update(Domain.Entities.Voucher voucher);
}
