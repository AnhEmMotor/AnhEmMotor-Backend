using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domains.Entities
{
    [Table("ProductStatus")]
    public class ProductStatus
    {
        [Key]
        [Column("Key")]
        public string? Key { get; set; }

        public ICollection<Product> Products { get; set; } = [];
    }
}