using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Setting")]
    public class Setting
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; }

        [Column("Value")]
        public long? Value { get; set; }
    }
}