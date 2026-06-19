using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.SupplierDebt
{
    public interface ISupplierDebtAuditLogInsertRepository
    {
        public Task InsertAsync(SupplierDebtAuditLog auditLog, CancellationToken cancellationToken);
    }
}
