using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Interfaces.Repositories.Invoice;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Admin.Invoices.Queries.GetAdminInvoices;

public record GetAdminInvoicesQuery(SieveModel SieveModel)
    : IRequest<Result<PagedResult<AdminInvoiceSummaryResponse>>>;

public class GetAdminInvoicesHandler(IInvoiceReadRepository readRepo)
    : IRequestHandler<GetAdminInvoicesQuery, Result<PagedResult<AdminInvoiceSummaryResponse>>>
{
  public async Task<Result<PagedResult<AdminInvoiceSummaryResponse>>> Handle(
      GetAdminInvoicesQuery request,
      CancellationToken cancellationToken)
  {
    var paged = await readRepo.GetPagedAsync(request.SieveModel, cancellationToken)
        .ConfigureAwait(false);
    return Result<PagedResult<AdminInvoiceSummaryResponse>>.Success(paged);
  }
}
