using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PromotionBanner")]
    public class PromotionBanner : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Title", TypeName = "nvarchar(150)")]
        public string Title { get; set; } = string.Empty;

        [Column("Description", TypeName = "nvarchar(500)")]
        public string? Description { get; set; }

        [Column("ImageUrl", TypeName = "nvarchar(500)")]
        public string? ImageUrl { get; set; }

        [Column("LinkUrl", TypeName = "nvarchar(500)")]
        public string? LinkUrl { get; set; }

        [Column("StartDate")]
        public DateTimeOffset StartDate { get; set; }

        [Column("EndDate")]
        public DateTimeOffset EndDate { get; set; }

        [Column("SortOrder")]
        public int SortOrder { get; set; }

        [Column("IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("UpdatedBy")]
        [ForeignKey("UpdatedByUser")]
        public Guid? UpdatedBy { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? UpdatedByUser { get; set; }
    }
}
