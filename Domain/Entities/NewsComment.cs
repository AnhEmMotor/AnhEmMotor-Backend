using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("NewsComments")]
    public class NewsComment : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("NewsId")]
        public int? NewsId { get; set; }

        [ForeignKey("NewsId")]
        public News? News { get; set; }

        [Column("ArticleType", TypeName = "nvarchar(50)")]
        public string? ArticleType { get; set; }

        [Column("ArticleSlug", TypeName = "nvarchar(255)")]
        public string? ArticleSlug { get; set; }

        [Column("UserId")]
        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Column("AuthorName", TypeName = "nvarchar(100)")]
        public string? AuthorName { get; set; }

        [Column("AuthorEmail", TypeName = "nvarchar(100)")]
        public string? AuthorEmail { get; set; }

        [Required]
        [Column("Content", TypeName = "nvarchar(max)")]
        public string Content { get; set; } = string.Empty;

        [Column("IsApproved")]
        public bool IsApproved { get; set; } = true;
    }
}
