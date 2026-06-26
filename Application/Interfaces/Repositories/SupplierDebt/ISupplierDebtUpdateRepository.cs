using Domain.Entities;

namespace Application.Interfaces.Repositories.SupplierDebt
{
    public interface ISupplierDebtUpdateRepository
    {
        public void Update(Domain.Entities.SupplierDebt supplierDebt);

        public void UpdateSupplierDebtLog(SupplierDebtLog supplierDebtLog);
    }
}
