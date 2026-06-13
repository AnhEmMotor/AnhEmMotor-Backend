using System;
using System.Collections.Generic;
using Application.ApiContracts.Logistics.Responses;
using MediatR;

namespace Application.Features.Logistics.Queries.GetShipmentTracking
{
    public class GetShipmentTrackingQuery : IRequest<ShipmentTrackingResponse>
    {
        public string? TrackingNumberOrPhone { get; set; }
    }

   

    
}
