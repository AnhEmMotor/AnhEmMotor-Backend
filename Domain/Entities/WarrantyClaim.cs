using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("WarrantyClaim")]
public class WarrantyClaim : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public string IssueDescription { get; set; } = string.Empty;

    public string? MediaUrls { get; set; }

    public string? ServiceCenterName { get; set; }

    public string? ManufacturerClaimNumber { get; set; }

    public int Status { get; set; }

    public string? ManufacturerDecision { get; set; }

    public bool IsRecall { get; set; }

    public decimal TotalPartsCost { get; set; }

    public decimal TotalLaborCost { get; set; }

    [NotMapped]
    public Vehicle? Vehicle { get; set; }

    public virtual ICollection<WarrantyClaimPart> WarrantyClaimParts { get; set; } = new List<WarrantyClaimPart>();
}
