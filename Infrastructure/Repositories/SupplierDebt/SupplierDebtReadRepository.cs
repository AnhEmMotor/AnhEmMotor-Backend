using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Infrastructure.Repositories.SupplierDebt
{
    public class SupplierDebtReadRepository(ApplicationDBContext context) : ISupplierDebtReadRepository
    {
        public Task<Domain.Entities.SupplierDebt?> GetByReceiptAndSupplierAsync(
            int receiptId,
            int supplierId,
            CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .FirstOrDefaultAsync(
                    d => d.InventoryReceiptId == receiptId && d.SupplierId == supplierId && d.DeletedAt == null,
                    cancellationToken);
        }

        public Task<Domain.Entities.SupplierDebt?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .ThenInclude(r => r!.InventoryReceiptInfos)
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        }

        public Task<List<Domain.Entities.SupplierDebt>> GetBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .ThenInclude(r => r!.InventoryReceiptInfos)
                .ThenInclude(i => i.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariant)
                .ThenInclude(v => v!.Product)
                .Include(d => d.InventoryReceipt)
                .ThenInclude(r => r!.InventoryReceiptInfos)
                .ThenInclude(i => i.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariantColor)
                .Where(d => d.SupplierId == supplierId && d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.SupplierDebt>> GetAllAsync(CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .Where(d => d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupplierDebtLog>> GetSupplierDebtLogsBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken)
        {
            return await context.SupplierDebtLogs
                .AsNoTracking()
                .Include(l => l.CreatedBy)
                .Include(l => l.ProofImages)
                .Where(l => l.SupplierId == supplierId)
                .OrderByDescending(l => l.PaymentDate)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public IQueryable<SupplierDebtLog> GetDebtLogsMissingProofsQueryable()
        {
            return context.SupplierDebtLogs
                .Include(x => x.Supplier)
                .Include(x => x.CreatedBy)
                .Where(x => !x.ProofImages.Any());
        }

        public Task<SupplierDebtLog?> GetDebtLogByIdAsync(int logId, CancellationToken cancellationToken)
        {
            return context.SupplierDebtLogs
                .Include(x => x.ProofImages)
                .FirstOrDefaultAsync(x => x.Id == logId, cancellationToken);
        }

        public async Task<List<SupplierDebtLogImage>> GetDebtLogProofImagesAsync(
            int debtLogId,
            CancellationToken cancellationToken)
        {
            return await context.SupplierDebtLogImages
                .Where(x => x.SupplierDebtLogId == debtLogId)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> IsDebtProofImageAsync(int mediaFileId, CancellationToken cancellationToken)
        {
            var suffix = $"/proof-image/{mediaFileId}";
            return context.SupplierDebtLogImages.AnyAsync(x => x.ImageUrl.EndsWith(suffix), cancellationToken);
        }
    }
}
