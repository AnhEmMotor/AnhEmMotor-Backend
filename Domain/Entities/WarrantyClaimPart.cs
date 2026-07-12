using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("WarrantyClaimPart")]
public class WarrantyClaimPart : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    public int WarrantyClaimId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string PartCode { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Status { get; set; }

    public virtual WarrantyClaim WarrantyClaim { get; set; } = null!;
}
