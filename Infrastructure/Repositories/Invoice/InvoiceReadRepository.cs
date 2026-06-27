using Application.Interfaces.Repositories.Invoice;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using InvoiceEntity = Domain.Entities.Invoice;

namespace Infrastructure.Repositories.Invoice;

public class InvoiceReadRepository(ApplicationDBContext context) : IInvoiceReadRepository
{
	public Task<List<InvoiceEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
		=> context.Set<InvoiceEntity>()
			.Where(x => x.UserId == userId && !x.DeletedAt.HasValue)
			.OrderByDescending(x => x.IssueDate)
			.ToListAsync(cancellationToken);

	public Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		=> context.Set<InvoiceEntity>()
			.FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue, cancellationToken);
}
