using Application.ApiContracts.Admin.Invoices;
using Domain.Primitives;
using Sieve.Models;
using InvoiceEntity = Domain.Entities.Invoice;

namespace Application.Interfaces.Repositories.Invoice;

public interface IInvoiceReadRepository
{
    public Task<PagedResult<AdminInvoiceSummaryResponse>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default);

    public Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<List<InvoiceEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
