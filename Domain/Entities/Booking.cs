using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Booking")]
public class Booking : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("FullName", TypeName = "nvarchar(100)")]
    public string FullName { get; set; } = string.Empty;

    [Column("Email", TypeName = "nvarchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("PhoneNumber", TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("ProductVariantId")]
    [ForeignKey("ProductVariant")]
    public int ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    [Column("PreferredDate")]
    public DateTimeOffset PreferredDate { get; set; }

    [Column("Note", TypeName = "nvarchar(MAX)")]
    public string Note { get; set; } = string.Empty;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = "Pending";

    [Column("BookingType", TypeName = "nvarchar(20)")]
    public string BookingType { get; set; } = "TestDrive";

    [Column("Location", TypeName = "nvarchar(200)")]
    public string Location { get; set; } = "Showroom";
}
