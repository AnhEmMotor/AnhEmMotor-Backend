using Domain.Entities;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierDebtRepository
    {
        public void Add(SupplierDebt supplierDebt);

        public void Update(SupplierDebt supplierDebt);

        public Task<SupplierDebt?> GetByReceiptAndSupplierAsync(
            int receiptId,
            int supplierId,
            CancellationToken cancellationToken);

        public Task<SupplierDebt?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<List<SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken);

        public Task<List<SupplierDebt>> GetAllAsync(CancellationToken cancellationToken);
    }
}

