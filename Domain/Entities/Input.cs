using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Input")]
    public class Input
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

        public InputStatus? InputStatus { get; set; }

        public Supplier? Supplier { get; set; }

        public ICollection<InputInfo> InputInfos { get; set; } = [];
    }
}