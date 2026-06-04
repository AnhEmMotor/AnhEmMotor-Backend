using System;

namespace Application.ApiContracts.DebtPayment.Responses
{
    public class InventoryReceiptDebtLineResponse
    {
        public int Id { get; set; }
        public int InventoryReceiptId { get; set; }
        public int? ProductVariantId { get; set; }
        public int? ProductVariantColorId { get; set; }
        public string? ProductVariantName { get; set; }
        public string? ColorName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }
}
