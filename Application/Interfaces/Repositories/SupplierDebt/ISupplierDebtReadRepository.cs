using Domain.Entities;

namespace Application.Interfaces.Repositories.SupplierDebt
{
    public interface ISupplierDebtReadRepository
    {
        public Task<Domain.Entities.SupplierDebt?> GetByReceiptAndSupplierAsync(
            int receiptId,
            int supplierId,
            CancellationToken cancellationToken);

        public Task<Domain.Entities.SupplierDebt?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<List<Domain.Entities.SupplierDebt>> GetBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken);

        public Task<List<Domain.Entities.SupplierDebt>> GetAllAsync(CancellationToken cancellationToken);

        public Task<List<SupplierDebtLog>> GetSupplierDebtLogsBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken);

        public IQueryable<SupplierDebtLog> GetDebtLogsMissingProofsQueryable();

        public Task<SupplierDebtLog?> GetDebtLogByIdAsync(int logId, CancellationToken cancellationToken);

        public Task<List<SupplierDebtLogImage>> GetDebtLogProofImagesAsync(int debtLogId, CancellationToken cancellationToken);

        public Task<bool> IsDebtProofImageAsync(int mediaFileId, CancellationToken cancellationToken);
    }
}
