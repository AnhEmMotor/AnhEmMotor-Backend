using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class TrackingItemResponse
    {
        public string? Sku { get; set; }

        public string? ProductName { get; set; }

        public int Quantity { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
