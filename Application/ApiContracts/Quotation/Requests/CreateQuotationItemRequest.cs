using System;

namespace Application.ApiContracts.Quotation.Requests
{
    public class CreateQuotationItemRequest
    {
        public string? ProductVariantId { get; set; }

        public string? ProductVarientColorId { get; set; }

        public int? QuotePrice { get; set; }

        public string? Note { get; set; }
    }
}
