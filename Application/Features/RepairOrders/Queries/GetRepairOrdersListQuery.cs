using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrdersListQuery : IRequest<Result<PagedResult<RepairOrderResponse>>>
{
    public SieveModel Sieve { get; set; } = new();
    public DataFetchMode Mode { get; set; } = DataFetchMode.ActiveOnly;
}
