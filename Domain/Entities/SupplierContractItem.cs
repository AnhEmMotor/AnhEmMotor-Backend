using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierContractItem")]
    public class SupplierContractItem : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SupplierContractId { get; set; }
        [ForeignKey("SupplierContractId")]
        public SupplierContract? SupplierContract { get; set; }

        public int ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        public ProductVariant? ProductVariant { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice { get; set; }
    }
}
