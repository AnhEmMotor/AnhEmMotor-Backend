using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("VehicleType")]
    public class VehicleType : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Slug")]
        public string? Slug { get; set; }

        [Column("ImageUrl")]
        public string? ImageUrl { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("SortOrder")]
        public int SortOrder { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = [];
    }
}
