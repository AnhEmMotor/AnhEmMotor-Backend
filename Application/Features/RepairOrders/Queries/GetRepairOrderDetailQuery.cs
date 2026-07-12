using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrderDetailQuery : IRequest<Result<RepairOrderResponse>>
{
    public int Id { get; set; }
}
