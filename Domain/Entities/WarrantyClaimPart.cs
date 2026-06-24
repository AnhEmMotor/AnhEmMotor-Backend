using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("WarrantyClaimPart")]
    public class WarrantyClaimPart : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("WarrantyClaimId")]
        [ForeignKey("WarrantyClaim")]
        public int WarrantyClaimId { get; set; }

        public WarrantyClaim? WarrantyClaim { get; set; }

        [Required]
        [Column("PartName", TypeName = "nvarchar(200)")]
        public string PartName { get; set; } = string.Empty;

        [Column("PartCode", TypeName = "nvarchar(100)")]
        public string PartCode { get; set; } = string.Empty;

        [Column("UnitPrice", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column("Status")]
        public WarrantyPartStatus Status { get; set; } = WarrantyPartStatus.Pending;
    }
}
