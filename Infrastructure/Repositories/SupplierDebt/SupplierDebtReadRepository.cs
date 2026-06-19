using Application.Interfaces.Repositories.SupplierDebt;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<List<Domain.Entities.SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
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

        public async Task<List<Domain.Entities.SupplierDebtAuditLog>> GetSupplierDebtAuditLogsAsync(int supplierDebtId, CancellationToken cancellationToken)
        {
            return await context.SupplierDebtAuditLogs
                .IgnoreQueryFilters()
                .Include(l => l.ChangedBy)
                .Where(l => l.SupplierDebtId == supplierDebtId)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
