using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("NewsCategory")]
    public class NewsCategory : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Name", TypeName = "nvarchar(255)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("Slug", TypeName = "varchar(255)")]
        public string Slug { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        public ICollection<News> News { get; set; } = [];
    }
}
