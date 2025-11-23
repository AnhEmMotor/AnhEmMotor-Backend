using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Brand")]
    public class Brand: BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = [];
    }
}