using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Banner")]
    public class Banner : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Title", TypeName = "nvarchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("ImageUrl", TypeName = "nvarchar(500)")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("LinkUrl", TypeName = "nvarchar(500)")]
        public string? LinkUrl { get; set; }

        [Column("Position", TypeName = "nvarchar(50)")]
        public string? Position { get; set; } // HomeTop, HomeMiddle, etc.

        [Column("StartDate")]
        public DateTimeOffset? StartDate { get; set; }

        [Column("EndDate")]
        public DateTimeOffset? EndDate { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
