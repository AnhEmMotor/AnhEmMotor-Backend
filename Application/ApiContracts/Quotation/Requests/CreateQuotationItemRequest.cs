using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Quotation.Requests
{
    public class CreateQuotationItemRequest
    {
        public string? ProductVariantId { get; set; }
        public string? ProductVarientColorId { get; set; }
        public int? QuotePrice { get; set; }
    }
}
