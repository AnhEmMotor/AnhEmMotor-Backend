using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrdersListQueryHandler(
    IMaintenanceHistoryReadRepository repo) : IRequestHandler<GetRepairOrdersListQuery, Result<PagedResult<RepairOrderResponse>>>
{
    public async Task<Result<PagedResult<RepairOrderResponse>>> Handle(GetRepairOrdersListQuery req, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync<RepairOrderResponse>(req.Sieve, req.Mode, null, ct);
        return Result<PagedResult<RepairOrderResponse>>.Success(paged);
    }
}
