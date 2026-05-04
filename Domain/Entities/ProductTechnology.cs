using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductTechnology : BaseEntity
    {
        public int ProductId { get; set; }

        public int TechnologyId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Column("CustomTitle", TypeName = "nvarchar(255)")]
        public string? CustomTitle { get; set; }

        [Column("CustomDescription", TypeName = "nvarchar(MAX)")]
        public string? CustomDescription { get; set; }

        [Column("CustomImageUrl", TypeName = "nvarchar(1000)")]
        public string? CustomImageUrl { get; set; }

        public Product? Product { get; set; }

        public Technology? Technology { get; set; }
    }
}
