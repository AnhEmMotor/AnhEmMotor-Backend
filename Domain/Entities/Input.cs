using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Input")]
    public class Input : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InputDate")]
        public DateTimeOffset? InputDate { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("StatusId")]
        [ForeignKey("InputStatus")]
        public string? StatusId { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("ConfirmedBy")]
        [ForeignKey("ConfirmedByUser")]
        public Guid? ConfirmedBy { get; set; }

        [Column("SourceOrderId")]
        [ForeignKey("Output")]
        public int? SourceOrderId { get; set; }

        public InputStatus? InputStatus { get; set; }

        public Output? Output { get; set; }

        public Supplier? Supplier { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? ConfirmedByUser { get; set; }

        public ICollection<InputInfo> InputInfos { get; set; } = [];
    }
}