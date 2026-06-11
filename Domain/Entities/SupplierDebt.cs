using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierDebt")]
    public class SupplierDebt : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InventoryReceiptId")]
        [ForeignKey("InventoryReceipt")]
        public int InventoryReceiptId { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column("PaidAmount", TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }

        public InventoryReceipt? InventoryReceipt { get; set; }

        public Supplier? Supplier { get; set; }
    }
}
