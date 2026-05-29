using System;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductVariantLiteResponseForInventoryReceipt
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public string? DisplayName { get; set; }

        public string? CoverImageUrl { get; set; }

        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }

        public string? ManagementType { get; set; }

        public List<ProductVariantColorLiteResponse> Colors { get; set; } = [];
    }
}
