using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseRequestItemAuditLog")]
    public class PurchaseRequestItemAuditLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseRequestItemId { get; set; }

        public string Action { get; set; } = string.Empty;

        public int? OldQuantity { get; set; }
        public int? NewQuantity { get; set; }
        
        public int? OldProductVariantId { get; set; }
        public int? NewProductVariantId { get; set; }
        
        public int? OldProductVariantColorId { get; set; }
        public int? NewProductVariantColorId { get; set; }

        public string? OldSupplierName { get; set; }
        public string? NewSupplierName { get; set; }

        [ForeignKey("PurchaseRequestItemId")]
        public virtual PurchaseRequestItem PurchaseRequestItem { get; set; } = null!;
    }
}
