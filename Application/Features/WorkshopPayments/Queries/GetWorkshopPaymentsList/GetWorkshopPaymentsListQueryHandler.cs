using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentsList;

public class GetWorkshopPaymentsListQueryHandler : IRequestHandler<GetWorkshopPaymentsListQuery, Result<PagedResult<object>>>
{
    public async Task<Result<PagedResult<object>>> Handle(GetWorkshopPaymentsListQuery request, CancellationToken cancellationToken)
    {
        // Mock implementation
        await Task.CompletedTask;
        var pagedResult = new PagedResult<object>(new System.Collections.Generic.List<object>(), 0, 1, 10);
        return Result<PagedResult<object>>.Success(pagedResult);
    }
}
