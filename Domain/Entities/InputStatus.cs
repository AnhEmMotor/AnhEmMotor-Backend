using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InputStatus")]
    public class InputStatus : BaseEntity
    {
        [Key]
        [Column("Key")]
        public string Key { get; set; } = string.Empty;

        public ICollection<Input> InputReceipts { get; set; } = [];
    }
}