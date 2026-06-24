using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.SupplierDebt
{
    public class SupplierDebtInsertRepository(ApplicationDBContext context) : ISupplierDebtInsertRepository
    {
        public void Add(Domain.Entities.SupplierDebt supplierDebt)
        {
            context.SupplierDebts.Add(supplierDebt);
        }

        public async Task InsertSupplierDebtLogAsync(
            SupplierDebtLog supplierDebtLog,
            CancellationToken cancellationToken)
        {
            await context.SupplierDebtLogs.AddAsync(supplierDebtLog, cancellationToken).ConfigureAwait(false);
        }
    }
}
