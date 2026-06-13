using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("RepairOrderDetail")]
    public class RepairOrderDetail : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("RepairOrderId")]
        [ForeignKey("RepairOrder")]
        public int RepairOrderId { get; set; }

        public RepairOrder RepairOrder { get; set; } = null!;

        [Column("ServiceId")]
        [ForeignKey("Service")]
        public int? ServiceId { get; set; }

        public Service? Service { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int? ProductVariantId { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        [Column("Count")]
        public int Count { get; set; } = 1;

        [Column("Price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column("LaborCost", TypeName = "decimal(18,2)")]
        public decimal LaborCost { get; set; }

        [Required]
        [Column("Type", TypeName = "nvarchar(20)")]
        public string Type { get; set; } = RepairOrderDetailType.Service;

        [Column("Notes", TypeName = "nvarchar(500)")]
        public string? Notes { get; set; }
    }
}
