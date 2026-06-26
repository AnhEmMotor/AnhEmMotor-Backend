
namespace Application.Interfaces.Repositories.SupplierDebt
{
    public interface ISupplierDebtUpdateRepository
    {
        public void Update(Domain.Entities.SupplierDebt supplierDebt);
        public void UpdateSupplierDebtLog(Domain.Entities.SupplierDebtLog supplierDebtLog);
    }
}
