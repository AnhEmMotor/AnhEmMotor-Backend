using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryReceiptInfo")]
    public class InventoryReceiptInfo : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InventoryReceiptId")]
        [ForeignKey("InventoryReceiptReceipt")]
        public int InventoryReceiptId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("RemainingCount")]
        public int? RemainingCount { get; set; }

        [Column("ParentOutputInfoId")]
        [ForeignKey("ParentOutputInfo")]
        public int? ParentOutputInfoId { get; set; }

        [Column("PurchaseRequestItemId")]
        [ForeignKey("PurchaseRequestItem")]
        public int? PurchaseRequestItemId { get; set; }

        [Column("QuotationProductRowId")]
        [ForeignKey("QuotationProductRow")]
        public int? QuotationProductRowId { get; set; }

        public InventoryReceipt? InventoryReceiptReceipt { get; set; }

        public PurchaseRequestItem? PurchaseRequestItem { get; set; }

        public QuotationProductRow? QuotationProductRow { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
