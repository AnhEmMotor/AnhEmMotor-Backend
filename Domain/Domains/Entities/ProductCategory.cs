using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domains.Entities
{
    [Table("ProductCategory")]
    public class ProductCategory
    {
        [Key]
        [Column("Id")]
        public int? Id { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}