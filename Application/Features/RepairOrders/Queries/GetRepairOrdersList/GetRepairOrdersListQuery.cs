using Application.ApiContracts.RepairOrder.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.RepairOrders.Queries.GetRepairOrdersList
{
    public class GetRepairOrdersListQuery : IRequest<Result<PagedResult<RepairOrderResponse>>>
    {
        public SieveModel? SieveModel { get; set; }
    }
}
