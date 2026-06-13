using Application.ApiContracts.Return.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Returns.Queries.GetReturns;

public class GetReturnsQuery : IRequest<List<ReturnOrderResponse>>
{
    public ReturnOrderStatus? Status { get; set; }
}



