using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryReceiptInfoAuditLog")]
    public class InventoryReceiptInfoAuditLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int InventoryReceiptInfoId { get; set; }

        public string Action { get; set; } = string.Empty;

        public int? OldQuantity { get; set; }

        public int? NewQuantity { get; set; }

        [ForeignKey("InventoryReceiptInfoId")]
        public virtual InventoryReceiptInfo InventoryReceiptInfo { get; set; } = null!;
    }
}
