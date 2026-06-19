using Application.Interfaces.Repositories.SupplierDebt;
using Infrastructure.DBContexts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.SupplierDebt
{
    public class SupplierDebtInsertRepository(ApplicationDBContext context) : ISupplierDebtInsertRepository
    {
        public void Add(Domain.Entities.SupplierDebt supplierDebt)
        {
            context.SupplierDebts.Add(supplierDebt);
        }
    }
}
