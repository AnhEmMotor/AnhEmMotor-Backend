namespace Domain.Entities;

public class VoucherLead
{
    public int VoucherId { get; set; }

    public virtual Voucher Voucher { get; set; } = null!;

    public int LeadId { get; set; }

    public virtual Lead Lead { get; set; } = null!;
}
