using AnhEmMotor.Application.Interfaces.Repositories.Invoice;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InvoiceEntity = Domain.Entities.Invoice;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceReadRepository : IInvoiceReadRepository
    {
        private readonly ApplicationDBContext _db;
        public InvoiceReadRepository(ApplicationDBContext db) => _db = db;

        public async Task<List<InvoiceEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _db.Set<InvoiceEntity>().Where(i => i.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _db.Set<InvoiceEntity>().FindAsync(new object[] { id }, cancellationToken);
        }
    }
}
