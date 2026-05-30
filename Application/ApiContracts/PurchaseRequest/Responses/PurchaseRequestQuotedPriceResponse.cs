namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestQuotedPriceResponse
    {
        public int ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public int QuotePrice { get; set; }

        public string? Note { get; set; }
    }
}
