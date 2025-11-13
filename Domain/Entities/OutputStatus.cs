using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OutputStatus")]
    public class OutputStatus
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; }

        public ICollection<Output> OutputOrders { get; set; } = [];
    }
}