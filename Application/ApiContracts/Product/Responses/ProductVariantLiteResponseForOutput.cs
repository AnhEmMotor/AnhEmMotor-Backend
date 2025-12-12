using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductVariantLiteResponseForOutput
    {
        public int? Id { get; set; }

        public string? DisplayName { get; set; }

        public string? CoverImageUrl { get; set; }

        public long Stock { get; set; }
    }
}
