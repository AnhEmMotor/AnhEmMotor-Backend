using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Setting")]
    public class Setting : BaseEntity
    {
        [Key]
        [Column("Key")]
        public string Key { get; set; } = string.Empty;

        [Column("Value", TypeName = "decimal(18, 2)")]
        public decimal? Value { get; set; }
    }
}