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
        [ForeignKey("InventoryReceipt")]
        public int InventoryReceiptId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("RemainingCount")]
        public int? RemainingCount { get; set; }

        [Column("ParentOutputInfoId")]
        [ForeignKey("ParentOutputInfo")]
        public int? ParentOutputInfoId { get; set; }

        [Column("PurchaseOrderItemId")]
        [ForeignKey("PurchaseOrderItem")]
        public int? PurchaseOrderItemId { get; set; }

        public InventoryReceipt? InventoryReceipt { get; set; }

        public PurchaseOrderItem? PurchaseOrderItem { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
