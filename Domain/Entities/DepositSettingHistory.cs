using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("DepositSettingHistory")]
    public class DepositSettingHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string OrderType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderThreshold { get; set; }

        public int DepositRatio { get; set; }
    }
}
