using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries;

public class GetWorkshopPaymentsListQueryHandler(
    IWorkshopPaymentReadRepository repo) : IRequestHandler<GetWorkshopPaymentsListQuery, Result<PagedResult<WorkshopPaymentResponse>>>
{
    public async Task<Result<PagedResult<WorkshopPaymentResponse>>> Handle(GetWorkshopPaymentsListQuery req, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync<WorkshopPaymentResponse>(req.Sieve, req.Mode, null, ct);
        return Result<PagedResult<WorkshopPaymentResponse>>.Success(paged);
    }
}
