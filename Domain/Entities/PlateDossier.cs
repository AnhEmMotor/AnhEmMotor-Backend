using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PlateDossier")]
    public class PlateDossier : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("OutputId")]
        [ForeignKey("Output")]
        public int OutputId { get; set; }

        public Output Output { get; set; } = null!;

        [Required]
        [Column("Status", TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "Prepare";

        [Column("LicensePlate", TypeName = "nvarchar(50)")]
        public string LicensePlate { get; set; } = string.Empty;

        [Column("RegistrationFee", TypeName = "decimal(18,2)")]
        public decimal RegistrationFee { get; set; }

        [Column("ActualCost", TypeName = "decimal(18,2)")]
        public decimal ActualCost { get; set; }

        [Column("ServiceFee", TypeName = "decimal(18,2)")]
        public decimal ServiceFee { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }
    }
}
