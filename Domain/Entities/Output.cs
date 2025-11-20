using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Output")]
    public class Output
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("CustomerName")]
        public string? CustomerName { get; set; }

        [Column("EmpCode")]
        public int? EmpCode { get; set; }

        [Column("StatusId")]
        [ForeignKey("OutputStatus")]
        public string? StatusId { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        public OutputStatus? OutputStatus { get; set; }

        public ICollection<OutputInfo> OutputInfos { get; set; } = [];
    }
}