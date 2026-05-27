using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Quotation.Requests
{
    public class UpdateQuotationItemRequest
    {
        public int? Id { get; set; }
        public string? ProductVariantId { get; set; }
        public string? ProductVarientColorId { get; set; }
        public int? QuotePrice { get; set; }
    }
}
