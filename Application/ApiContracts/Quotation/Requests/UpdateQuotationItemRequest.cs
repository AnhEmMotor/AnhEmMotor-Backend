using System;

namespace Application.ApiContracts.Quotation.Requests
{
    public class UpdateQuotationItemRequest
    {
        public int? Id { get; set; }

        public string? ProductVariantId { get; set; }

        public string? ProductVarientColorId { get; set; }

        public int? QuotePrice { get; set; }

        public string? Note { get; set; }
    }
}
