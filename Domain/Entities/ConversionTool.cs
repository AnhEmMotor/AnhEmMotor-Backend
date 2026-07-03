using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ConversionTool")]
public class ConversionTool : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Type", TypeName = "nvarchar(20)")]
    public string Type { get; set; } = "Popup";

    [Column("Name", TypeName = "nvarchar(200)")]
    public string Name { get; set; } = string.Empty;

    [Column("Content", TypeName = "nvarchar(MAX)")]
    public string? Content { get; set; }

    [Column("DelaySeconds")]
    public int? DelaySeconds { get; set; }

    [Column("Pages", TypeName = "nvarchar(MAX)")]
    public string? Pages { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("Views")]
    public int Views { get; set; }

    [Column("Clicks")]
    public int Clicks { get; set; }

    [Column("ImageUrl", TypeName = "nvarchar(500)")]
    public string? ImageUrl { get; set; }

    [Column("Url", TypeName = "nvarchar(500)")]
    public string? Url { get; set; }

    [Column("Status", TypeName = "nvarchar(20)")]
    public string? Status { get; set; }

    [Column("Leads")]
    public int Leads { get; set; }
}
