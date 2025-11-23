using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("MediaFile")]
    public class MediaFile : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [Required]
        [StringLength(255)]
        public string? StoredFileName { get; set; }

        [Required]
        [StringLength(100)]
        public string? ContentType { get; set; }

        [StringLength(1024)]
        public string? PublicUrl { get; set; }

        public long? FileSize { get; set; }
    }
}
