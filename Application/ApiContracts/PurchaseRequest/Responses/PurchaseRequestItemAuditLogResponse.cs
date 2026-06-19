namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestItemAuditLogResponse
    {
        public int Id { get; set; }
        public int PurchaseRequestItemId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? OldQuantity { get; set; }
        public int? NewQuantity { get; set; }
        public int? OldProductVariantId { get; set; }
        public int? NewProductVariantId { get; set; }
        public string? OldProductVariantName { get; set; }
        public string? NewProductVariantName { get; set; }
        public int? OldProductVariantColorId { get; set; }
        public int? NewProductVariantColorId { get; set; }
        public string? OldProductVariantColorName { get; set; }
        public string? NewProductVariantColorName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
