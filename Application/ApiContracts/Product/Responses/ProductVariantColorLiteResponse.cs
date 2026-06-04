using System;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductVariantColorLiteResponse
    {
        public int Id { get; set; }

        public string? ColorName { get; set; }

        public string? ColorCode { get; set; }

        public string? CoverImageUrl { get; set; }

        public int? MaxPurchaseQuantity { get; set; }
    }
}
