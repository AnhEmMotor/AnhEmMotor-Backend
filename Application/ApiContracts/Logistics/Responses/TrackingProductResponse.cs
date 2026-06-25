using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class TrackingProductResponse
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
