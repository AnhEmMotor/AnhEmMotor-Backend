using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
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

        public async Task<SupplierDebt?> GetByReceiptAndSupplierAsync(int receiptId, int supplierId, CancellationToken cancellationToken)
        {
            return await context.SupplierDebts
                .Include(sd => sd.InventoryReceipt)
                    .ThenInclude(ir => ir!.InventoryReceiptInfos)
                        .ThenInclude(ii => ii.QuotationProductRow)
                            .ThenInclude(qp => qp!.QuotationReceipt)
                                .ThenInclude(q => q!.Supplier)
                .Include(sd => sd.Supplier)
                .FirstOrDefaultAsync(sd => sd.InventoryReceiptId == receiptId && sd.SupplierId == supplierId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
        {
            return await context.SupplierDebts
                .Include(sd => sd.InventoryReceipt)
                    .ThenInclude(ir => ir!.InventoryReceiptInfos)
                        .ThenInclude(ii => ii.QuotationProductRow)
                            .ThenInclude(qp => qp!.QuotationReceipt)
                                .ThenInclude(q => q!.Supplier)
                .Include(sd => sd.Supplier)
                .Where(sd => sd.SupplierId == supplierId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<SupplierDebt>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.SupplierDebts
                .Include(sd => sd.InventoryReceipt)
                    .ThenInclude(ir => ir!.InventoryReceiptInfos)
                        .ThenInclude(ii => ii.QuotationProductRow)
                            .ThenInclude(qp => qp!.QuotationReceipt)
                                .ThenInclude(q => q!.Supplier)
                .Include(sd => sd.Supplier)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
