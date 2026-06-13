using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Logistics.Queries.GetActiveShipments;

public class GetActiveShipmentsQuery : IRequest<List<ActiveShipmentResponse>>
{
}



