using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Voucher : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public VoucherApplyFor ApplyFor { get; set; }
    public VoucherChannel Channel { get; set; }
    public VoucherType Type { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    public virtual ICollection<VoucherLead> VoucherLeads { get; set; } = new List<VoucherLead>();
}
