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

        [Column("PurchaseRequestItemId")]
        [ForeignKey("PurchaseRequestItem")]
        public int? PurchaseRequestItemId { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18, 2)")]
        public decimal? UnitPrice { get; set; }

        public InventoryReceipt? InventoryReceipt { get; set; }

        public PurchaseRequestItem? PurchaseRequestItem { get; set; }

        public Supplier? Supplier { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
