using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Quotation.Responses
{
    public class QuotationItemResponse
    {
        public int? ProductVariantId { get; set; }

        public string? ProductVariantDisplayName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ProductVariantColorDisplayName { get; set; }

        public int? QuotePrice { get; set; }

        public string? Note { get; set; }
    }
}
