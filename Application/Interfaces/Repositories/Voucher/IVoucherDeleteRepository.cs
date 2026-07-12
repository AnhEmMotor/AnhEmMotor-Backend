
namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherDeleteRepository
{
    public void SoftDelete(Domain.Entities.Voucher voucher);
}
