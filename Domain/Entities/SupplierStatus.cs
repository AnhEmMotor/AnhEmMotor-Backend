using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierStatus")]
    public class SupplierStatus
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; }

        public ICollection<Supplier> Suppliers { get; set; } = [];
    }
}