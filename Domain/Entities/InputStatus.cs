using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InputStatus")]
    public class InputStatus
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; }

        public ICollection<Input> InputReceipts { get; set; } = [];
    }
}