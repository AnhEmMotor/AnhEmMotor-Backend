using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierDebtRepository(ApplicationDBContext context) : ISupplierDebtRepository
    {
        public void Add(SupplierDebt supplierDebt)
        {
            context.SupplierDebts.Add(supplierDebt);
        }

        public void Update(SupplierDebt supplierDebt)
        {
            context.SupplierDebts.Update(supplierDebt);
        }

        public Task<SupplierDebt?> GetByReceiptAndSupplierAsync(int receiptId, int supplierId, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .FirstOrDefaultAsync(d => d.InventoryReceiptId == receiptId && d.SupplierId == supplierId && d.DeletedAt == null, cancellationToken);
        }

        public Task<SupplierDebt?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                    .ThenInclude(r => r.InventoryReceiptInfos)
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        }

        public Task<List<SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                    .ThenInclude(r => r.InventoryReceiptInfos)
                        .ThenInclude(i => i.PurchaseRequestItem)
                            .ThenInclude(pri => pri.ProductVariant)
                                .ThenInclude(v => v.Product)
                .Include(d => d.InventoryReceipt)
                    .ThenInclude(r => r.InventoryReceiptInfos)
                        .ThenInclude(i => i.PurchaseRequestItem)
                            .ThenInclude(pri => pri.ProductVariantColor)
                .Where(d => d.SupplierId == supplierId && d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }

        public Task<List<SupplierDebt>> GetAllAsync(CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.InventoryReceipt)
                .Where(d => d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }
    }
}
