using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InvoiceEntity = Domain.Entities.Invoice;

namespace Application.Interfaces.Repositories.Invoice
{
    public interface IInvoiceReadRepository
    {
        public Task<List<InvoiceEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
        public Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
    }
}
