using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Technology : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [Column("Name", TypeName = "nvarchar(255)")]
        public string Name { get; set; } = string.Empty;

        [Column("DefaultTitle", TypeName = "nvarchar(255)")]
        public string? DefaultTitle { get; set; }

        [Column("DefaultDescription", TypeName = "nvarchar(MAX)")]
        public string? DefaultDescription { get; set; }

        [Column("DefaultImageUrl", TypeName = "nvarchar(1000)")]
        public string? DefaultImageUrl { get; set; }

        public TechnologyCategory? Category { get; set; }

        public ICollection<TechnologyImage> Images { get; set; } = [];
        public ICollection<ProductTechnology> ProductTechnologies { get; set; } = [];
    }
}
