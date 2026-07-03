using Application.ApiContracts.Admin.Invoices;
using Domain.Primitives;
using InvoiceEntity = Domain.Entities.Invoice;
using Sieve.Models;

namespace Application.Interfaces.Repositories.Invoice;

public interface IInvoiceReadRepository
{
  Task<PagedResult<AdminInvoiceSummaryResponse>> GetPagedAsync(
      SieveModel sieveModel,
      CancellationToken cancellationToken = default);

  Task<InvoiceEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

  Task<List<InvoiceEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
