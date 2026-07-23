using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("WarrantyTerm")]
public class WarrantyTerm : BaseEntity
{
	[Key]
	[Column("Id")]
	public int Id { get; set; }

	[Column("BrandId")]
	public int BrandId { get; set; }

	[ForeignKey("BrandId")]
	public virtual Brand? Brand { get; set; }

	[Column("TermName", TypeName = "nvarchar(200)")]
	public string TermName { get; set; } = string.Empty;

	[Column("TermNameJson", TypeName = "nvarchar(max)")]
	public string? TermNameJson { get; set; }

	[Column("VehicleType", TypeName = "nvarchar(100)")]
	public string VehicleType { get; set; } = string.Empty;

	[Column("ErrorCategory", TypeName = "nvarchar(200)")]
	public string ErrorCategory { get; set; } = string.Empty;

	[Column("Description", TypeName = "nvarchar(MAX)")]
	public string? Description { get; set; }

	[Column("DescriptionJson", TypeName = "nvarchar(max)")]
	public string? DescriptionJson { get; set; }

	[Column("DurationMonths")]
	public int? DurationMonths { get; set; }

	[Column("DurationKm")]
	public int? DurationKm { get; set; }

	[Column("Coverage", TypeName = "nvarchar(200)")]
	public string? Coverage { get; set; }

	[Column("Status", TypeName = "nvarchar(50)")]
	public string Status { get; set; } = WarrantyTermStatus.Active;

	[Column("EffectiveDate")]
	public DateTime? EffectiveDate { get; set; }

	[Column("ExpirationDate")]
	public DateTime? ExpirationDate { get; set; }

	[Column("MediaUrl", TypeName = "nvarchar(1000)")]
	public string? MediaUrl { get; set; }

	[Column("RowVersion")]
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}
