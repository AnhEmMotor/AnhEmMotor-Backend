using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("NewsArticle")]
    public class NewsArticle : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Title", TypeName = "nvarchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Column("Slug", TypeName = "nvarchar(255)")]
        public string Slug { get; set; } = string.Empty;

        [Column("Excerpt", TypeName = "nvarchar(500)")]
        public string? Excerpt { get; set; }

        [Column("Content", TypeName = "nvarchar(MAX)")]
        public string? Content { get; set; }

        [Column("CoverImageUrl", TypeName = "nvarchar(500)")]
        public string? CoverImageUrl { get; set; }

        [Column("SeoTitle", TypeName = "nvarchar(255)")]
        public string? SeoTitle { get; set; }

        [Column("SeoDescription", TypeName = "nvarchar(500)")]
        public string? SeoDescription { get; set; }

        [Column("SeoKeywords", TypeName = "nvarchar(500)")]
        public string? SeoKeywords { get; set; }

        [Column("Status", TypeName = "nvarchar(30)")]
        public string Status { get; set; } = "draft";

        [Column("IsFeatured")]
        public bool IsFeatured { get; set; }

        [Column("ViewCount")]
        public int ViewCount { get; set; }

        [Column("PublishedAt")]
        public DateTimeOffset? PublishedAt { get; set; }

        [Column("PublishedBy")]
        [ForeignKey("PublishedByUser")]
        public Guid? PublishedBy { get; set; }

        [Column("AuthorId")]
        [ForeignKey("Author")]
        public Guid? AuthorId { get; set; }

        public ApplicationUser? PublishedByUser { get; set; }

        public ApplicationUser? Author { get; set; }
    }
}
