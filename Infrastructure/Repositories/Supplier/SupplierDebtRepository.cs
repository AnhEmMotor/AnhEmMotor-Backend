using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Infrastructure.DBContexts;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Task<List<SupplierDebt>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<SupplierDebt>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
