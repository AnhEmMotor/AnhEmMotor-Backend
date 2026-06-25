using Application.ApiContracts.Logistics.Responses;
using MediatR;
using System;

namespace Application.Features.Logistics.Queries.GetShipmentTracking
{
    public class GetShipmentTrackingQuery : IRequest<ShipmentTrackingResponse>
    {
        public string? TrackingNumberOrPhone { get; set; }
    }
}
