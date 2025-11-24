using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("MediaFiles")]
public class MediaFile : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string? StorageType { get; set; } = "local";

    [StringLength(500)]
    public string? StoragePath { get; set; }

    [StringLength(255)]
    public string? OriginalFileName { get; set; }

    [StringLength(100)]
    public string? ContentType { get; set; }

    [StringLength(100)]
    public string? FileExtension { get; set; }

    public long? FileSize { get; set; }
}
