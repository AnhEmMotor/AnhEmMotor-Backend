using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities
{
    [Table("WarrantyClaim")]
    public class WarrantyClaim : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("ClaimNumber", TypeName = "nvarchar(50)")]
        public string ClaimNumber { get; set; } = string.Empty;

        [Column("VehicleId")]
        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        public Vehicle? Vehicle { get; set; }

        [Required]
        [Column("IssueDescription", TypeName = "nvarchar(MAX)")]
        public string IssueDescription { get; set; } = string.Empty;

        [Column("MediaUrls", TypeName = "nvarchar(MAX)")]
        public string? MediaUrls { get; set; }

        [Column("ServiceCenterName", TypeName = "nvarchar(200)")]
        public string? ServiceCenterName { get; set; }

        [Column("ManufacturerClaimNumber", TypeName = "nvarchar(100)")]
        public string? ManufacturerClaimNumber { get; set; }

        [Column("Status")]
        public WarrantyClaimStatus Status { get; set; } = WarrantyClaimStatus.Received;

        [Column("ManufacturerDecision", TypeName = "nvarchar(MAX)")]
        public string? ManufacturerDecision { get; set; }

        [Column("IsRecall")]
        public bool IsRecall { get; set; }

        [Column("TotalPartsCost", TypeName = "decimal(18,2)")]
        public decimal TotalPartsCost { get; set; }

        [Column("TotalLaborCost", TypeName = "decimal(18,2)")]
        public decimal TotalLaborCost { get; set; }

        public ICollection<WarrantyClaimPart> Parts { get; set; } = new List<WarrantyClaimPart>();
    }
}