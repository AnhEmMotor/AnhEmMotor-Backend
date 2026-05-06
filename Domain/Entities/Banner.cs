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

        [Required]
        [Column("MobileImageUrl", TypeName = "nvarchar(500)")]
        public string MobileImageUrl { get; set; } = string.Empty;

        [Column("LinkUrl", TypeName = "nvarchar(500)")]
        public string? LinkUrl { get; set; }

        [Column("CtaText", TypeName = "nvarchar(100)")]
        public string? CtaText { get; set; }

        [Column("Placement", TypeName = "nvarchar(50)")]
        public string? Placement { get; set; }

        [Column("StartDate")]
        public DateTimeOffset? StartDate { get; set; }

        [Column("EndDate")]
        public DateTimeOffset? EndDate { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("Priority")]
        public int Priority { get; set; }

        [Column("ClickCount")]
        public int ClickCount { get; set; } = 0;

        [Column("ViewCount")]
        public int ViewCount { get; set; } = 0;

        [NotMapped]
        public double CTR => ViewCount > 0 ? Math.Round((double)ClickCount / ViewCount * 100, 2) : 0;
    }
}
