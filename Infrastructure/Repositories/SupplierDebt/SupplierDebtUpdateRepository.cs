using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.SupplierDebt
{
    public class SupplierDebtUpdateRepository(ApplicationDBContext context) : ISupplierDebtUpdateRepository
    {
        public void Update(Domain.Entities.SupplierDebt supplierDebt)
        {
            context.SupplierDebts.Update(supplierDebt);
        }

        public void UpdateSupplierDebtLog(SupplierDebtLog supplierDebtLog)
        {
            context.SupplierDebtLogs.Update(supplierDebtLog);
        }
    }
}
