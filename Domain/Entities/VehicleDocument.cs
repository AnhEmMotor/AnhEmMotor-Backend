using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("VehicleDocument")]
public class VehicleDocument : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("VehicleId")]
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    [Column("DocumentType", TypeName = "nvarchar(50)")]
    public string DocumentType { get; set; } = string.Empty; // Registration, Insurance, Invoice

    [Column("FileUrl", TypeName = "nvarchar(500)")]
    public string FileUrl { get; set; } = string.Empty;

    [Column("Description", TypeName = "nvarchar(1000)")]
    public string Description { get; set; } = string.Empty;
}
