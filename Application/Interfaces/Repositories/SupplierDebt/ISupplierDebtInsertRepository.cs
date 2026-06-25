using Domain.Entities;

namespace Application.Interfaces.Repositories.SupplierDebt
{
    public interface ISupplierDebtInsertRepository
    {
        public void Add(Domain.Entities.SupplierDebt supplierDebt);

        public Task InsertSupplierDebtLogAsync(SupplierDebtLog supplierDebtLog, CancellationToken cancellationToken);
    }
}
