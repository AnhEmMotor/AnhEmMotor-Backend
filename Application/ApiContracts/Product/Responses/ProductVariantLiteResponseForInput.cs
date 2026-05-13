using System;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductVariantLiteResponseForInput
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public string? DisplayName { get; set; }

        public string? CoverImageUrl { get; set; }

        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }
    }
}
