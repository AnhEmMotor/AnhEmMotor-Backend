using Application.ApiContracts.RepairOrder.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Queries.GetRepairOrderDetail
{
    public class GetRepairOrderDetailQuery : IRequest<Result<RepairOrderResponse>>
    {
        public int Id { get; set; }
    }
}
