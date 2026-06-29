using System;
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
        public int? OutputId { get; set; }

        public Output? Output { get; set; }

        [Required]
        [Column("Status", TypeName = "nvarchar(50)")]
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

        // New fields for dossier management
        [Column("DossierNumber", TypeName = "nvarchar(50)")]
        public string DossierNumber { get; set; } = string.Empty;

        [Column("CustomerName", TypeName = "nvarchar(100)")]
        public string CustomerName { get; set; } = string.Empty;

        [Column("CustomerPhone", TypeName = "nvarchar(20)")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Column("VinNumber", TypeName = "nvarchar(50)")]
        public string VinNumber { get; set; } = string.Empty;

        [Column("CompletedDate")]
        public DateTimeOffset? CompletedDate { get; set; }
    }
}
