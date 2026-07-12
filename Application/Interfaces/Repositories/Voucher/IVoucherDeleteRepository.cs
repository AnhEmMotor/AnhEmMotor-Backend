
namespace Application.Interfaces.Repositories.Voucher;

public interface IVoucherDeleteRepository
{
    void SoftDelete(Domain.Entities.Voucher voucher);
}
