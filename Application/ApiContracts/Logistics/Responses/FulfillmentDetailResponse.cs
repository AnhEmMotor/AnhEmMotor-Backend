using Domain.Entities.Logistics;
using Domain.Enums;
using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class FulfillmentDetailResponse
    {
        public int Id { get; set; }

        public string TrackingNumber { get; set; } = string.Empty;

        public string OriginalOrderCode { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerAddress { get; set; } = string.Empty;

        public string Carrier { get; set; } = string.Empty;

        public ParcelDeliveryStatus Status { get; set; }

        public decimal CodAmount { get; set; }

        public decimal ShippingCost { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpectedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public List<FulfillmentDetailItemResponse> Items { get; set; } = [];
    }
}
