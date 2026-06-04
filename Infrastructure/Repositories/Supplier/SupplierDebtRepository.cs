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
                .Include(d => d.PurchaseInvoice)
                .FirstOrDefaultAsync(d => d.PurchaseInvoiceId == receiptId && d.SupplierId == supplierId && d.DeletedAt == null, cancellationToken);
        }

        public Task<SupplierDebt?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.PurchaseInvoice)
                    .ThenInclude(p => p.PurchaseInvoiceItems)
                        .ThenInclude(i => i.InventoryReceiptInfo)
                            .ThenInclude(iri => iri.InventoryReceipt)
                .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        }

        public Task<List<SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.PurchaseInvoice)
                    .ThenInclude(p => p.PurchaseInvoiceItems)
                        .ThenInclude(i => i.ProductVariant)
                            .ThenInclude(v => v.Product)
                .Include(d => d.PurchaseInvoice)
                    .ThenInclude(p => p.PurchaseInvoiceItems)
                        .ThenInclude(i => i.ProductVariantColor)
                .Include(d => d.PurchaseInvoice)
                    .ThenInclude(p => p.PurchaseInvoiceItems)
                        .ThenInclude(i => i.InventoryReceiptInfo)
                            .ThenInclude(iri => iri.InventoryReceipt)
                .Where(d => d.SupplierId == supplierId && d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }

        public Task<List<SupplierDebt>> GetAllAsync(CancellationToken cancellationToken)
        {
            return context.SupplierDebts
                .Include(d => d.Supplier)
                .Include(d => d.PurchaseInvoice)
                .Where(d => d.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }
    }
}
