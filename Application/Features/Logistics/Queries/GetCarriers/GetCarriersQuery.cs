using System.Collections.Generic;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using MediatR;

namespace Application.Features.Logistics.Queries.GetCarriers;

public class GetCarriersQuery : IRequest<GetCarriersResponse>
{
}

