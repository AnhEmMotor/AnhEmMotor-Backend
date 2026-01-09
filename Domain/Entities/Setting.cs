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

        [Column("Value", TypeName = "nvarchar(MAX)")]
        public string? Value { get; set; }
    }
}