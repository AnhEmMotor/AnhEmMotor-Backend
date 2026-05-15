using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("PartnerType")]
public class PartnerType : BaseEntity
{
    [Key]
    [Column("Key", TypeName = "nvarchar(50)")]
    public string Key { get; set; } = string.Empty;

    public ICollection<Supplier> Suppliers { get; set; } = [];
}
