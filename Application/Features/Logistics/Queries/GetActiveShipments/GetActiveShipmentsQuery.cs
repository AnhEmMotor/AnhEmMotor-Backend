using Application.ApiContracts.Logistics.Responses;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetActiveShipments;

public class GetActiveShipmentsQuery : IRequest<List<ActiveShipmentResponse>>
{
}

