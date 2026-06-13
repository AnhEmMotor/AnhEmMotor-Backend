using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class FulfillmentDetailItemResponse
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        public string ShelfLocation { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public bool IsPicked { get; set; }

        public bool IsRestricted { get; set; }

        public bool IsOutOfStock { get; set; }
    }
}
