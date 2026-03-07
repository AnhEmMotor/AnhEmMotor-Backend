using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("PredefinedOption")]
public class PredefinedOption : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Key", TypeName = "nvarchar(100)")]
    public string Key { get; set; } = string.Empty;

    [Column("Value", TypeName = "nvarchar(200)")]
    public string Value { get; set; } = string.Empty;
}
