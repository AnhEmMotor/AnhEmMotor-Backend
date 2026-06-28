using Application.ApiContracts.Admin.Invoices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Domain.Primitives;
using Infrastructure.DBContexts;
using InvoiceEntity = Domain.Entities.Invoice;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;

namespace Infrastructure.Repositories.Invoice;

public class InvoiceReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IInvoiceReadRepository
{
    public Task<PagedResult<AdminInvoiceSummaryResponse>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<InvoiceEntity>().Where(x => !x.DeletedAt.HasValue);
        return paginator.ApplyAsync<InvoiceEntity, AdminInvoiceSummaryResponse>(
            query, sieveModel, null, cancellationToken);
    }

    public Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.Set<InvoiceEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue, cancellationToken);
    }

    public Task<List<InvoiceEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Set<InvoiceEntity>()
            .Where(x => x.UserId == userId && !x.DeletedAt.HasValue)
            .OrderByDescending(x => x.IssueDate)
            .ToListAsync(cancellationToken);
    }
}
