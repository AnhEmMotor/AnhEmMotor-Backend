using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("SupplierDebtLogImages")]
public class SupplierDebtLogImage : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int SupplierDebtLogId { get; set; }

    [MaxLength(2000)]
    public string ImageUrl { get; set; } = null!;

    [ForeignKey("SupplierDebtLogId")]
    public virtual SupplierDebtLog SupplierDebtLog { get; set; } = null!;
}
