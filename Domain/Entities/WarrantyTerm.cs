using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("WarrantyTerm")]
    public class WarrantyTerm : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("BrandId", TypeName = "int")]
        public int BrandId { get; set; }

        [Column("TermName", TypeName = "nvarchar(200)")]
        public string? TermName { get; set; }

        [Column("TermNameJson", TypeName = "nvarchar(max)")]
        public string? TermNameJson { get; set; }

        [Column("VehicleType", TypeName = "nvarchar(200)")]
        public string? VehicleType { get; set; }

        [Column("ErrorCategory", TypeName = "nvarchar(200)")]
        public string? ErrorCategory { get; set; }

        [Column("Description", TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Column("DescriptionJson", TypeName = "nvarchar(max)")]
        public string? DescriptionJson { get; set; }

        [Column("DurationMonths", TypeName = "int")]
        public int? DurationMonths { get; set; }

        [Column("DurationKm", TypeName = "int")]
        public int? DurationKm { get; set; }

        [Column("Coverage", TypeName = "nvarchar(max)")]
        public string? Coverage { get; set; }

        [Column("Status", TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "Active";

        [Column("EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Column("ExpirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [Column("MediaUrl", TypeName = "nvarchar(500)")]
        public string? MediaUrl { get; set; }

        [Column("RowVersion", TypeName = "rowversion")]
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
