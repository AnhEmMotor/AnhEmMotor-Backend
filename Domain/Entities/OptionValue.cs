using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OptionValue")]
    public class OptionValue
    {
        [Key]
        [Column("Id")]
        public int? Id { get; set; }

        [Column("OptionId")]
        [ForeignKey("Option")]
        public int? OptionId { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        public Option? Option { get; set; }

        public ICollection<VariantOptionValue> VariantOptionValues { get; set; } = [];
    }
}