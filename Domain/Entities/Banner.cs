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
        [Column("DesktopImageUrl", TypeName = "nvarchar(500)")]
        public string DesktopImageUrl { get; set; } = string.Empty;

        [Column("MobileImageUrl", TypeName = "nvarchar(500)")]
        public string? MobileImageUrl { get; set; }

        [Column("Description", TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }

        [Column("CtaLink", TypeName = "nvarchar(500)")]
        public string? CtaLink { get; set; }

        [Column("CtaLabel", TypeName = "nvarchar(100)")]
        public string? CtaLabel { get; set; }

        [Column("Placement", TypeName = "nvarchar(50)")]
        public string? Placement { get; set; }


    }
}
