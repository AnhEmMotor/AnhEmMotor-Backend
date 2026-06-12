using System;
using System.Collections.Generic;
using MediatR;

namespace Application.Features.Logistics.Queries.GetShipmentTracking
{
    public class GetShipmentTrackingQuery : IRequest<ShipmentTrackingDto>
    {
        public string TrackingNumberOrPhone { get; set; }
    }

    public class ShipmentTrackingDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        
        public decimal TotalValue { get; set; }
        public decimal CodAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public int Status { get; set; }

        public List<TrackingItemDto> Items { get; set; } = new();
        public List<TrackingMilestoneDto> Milestones { get; set; } = new();
    }

    public class TrackingItemDto
    {
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class TrackingMilestoneDto
    {
        public string Timestamp { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsCurrent { get; set; }
        public string StatusType { get; set; } // e.g. "Completed", "InTransit", "Pending"
    }
}
