using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using Infrastructure.DBContexts;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.SupplierDebt
{
    public class SupplierDebtAuditLogInsertRepository(ApplicationDBContext context) : ISupplierDebtAuditLogInsertRepository
    {
        public async Task InsertAsync(SupplierDebtAuditLog auditLog, CancellationToken cancellationToken)
        {
            await context.SupplierDebtAuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);
        }
    }
}
