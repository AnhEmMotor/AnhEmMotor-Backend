using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductCategory")]
    public class ProductCategory : BaseEntity
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

        [Column("CategoryGroup")]
        public string? CategoryGroup { get; set; } // "Vehicle" or "Product"

        [Column("ParentId")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public ProductCategory? Parent { get; set; }

        public ICollection<ProductCategory> SubCategories { get; set; } = [];

        public ICollection<Product> Products { get; set; } = [];
    }
}