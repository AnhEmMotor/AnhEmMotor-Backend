using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OrderStatusHistory")]
    public class OrderStatusHistory : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("OutputId")]
        [ForeignKey("Output")]
        public int OutputId { get; set; }

        [Column("FromStatus", TypeName = "nvarchar(50)")]
        public string? FromStatus { get; set; }

        [Column("ToStatus", TypeName = "nvarchar(50)")]
        public string ToStatus { get; set; } = string.Empty;

        [Column("ChangedBy")]
        [ForeignKey("ChangedByUser")]
        public Guid? ChangedBy { get; set; }

        [Column("ChangedAt")]
        public DateTimeOffset? ChangedAt { get; set; }

        [Column("Note", TypeName = "nvarchar(500)")]
        public string? Note { get; set; }

        public Output? Output { get; set; }

        public ApplicationUser? ChangedByUser { get; set; }
    }
}
