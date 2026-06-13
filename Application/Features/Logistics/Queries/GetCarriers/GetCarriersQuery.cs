using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Logistics.Queries.GetCarriers;

public class GetCarriersQuery : IRequest<Result<CarrierPartnerResponse>>
{
}

