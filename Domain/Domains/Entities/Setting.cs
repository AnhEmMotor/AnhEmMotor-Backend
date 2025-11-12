using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domains.Entities
{
    [Table("Setting")]
    public class Setting
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; } = null!;

        [Column("Value")]
        public long? Value { get; set; }
    }
}