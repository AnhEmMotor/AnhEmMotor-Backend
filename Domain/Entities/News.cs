using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("News")]
    public class News : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Title", TypeName = "nvarchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("Slug", TypeName = "varchar(255)")]
        public string Slug { get; set; } = string.Empty;

        [Column("Content", TypeName = "nvarchar(max)")]
        public string? Content { get; set; }

        [Column("CoverImageUrl", TypeName = "nvarchar(500)")]
        public string? CoverImageUrl { get; set; }

        [Column("AuthorName", TypeName = "nvarchar(100)")]
        public string? AuthorName { get; set; }

        [Column("PublishedDate")]
        public DateTimeOffset? PublishedDate { get; set; }

        [Column("IsPublished")]
        public bool IsPublished { get; set; }

        // SEO Metadata
        [Column("MetaTitle", TypeName = "nvarchar(100)")]
        public string? MetaTitle { get; set; }

        [Column("MetaDescription", TypeName = "nvarchar(255)")]
        public string? MetaDescription { get; set; }

        [Column("MetaKeywords", TypeName = "nvarchar(255)")]
        public string? MetaKeywords { get; set; }
    }
}
