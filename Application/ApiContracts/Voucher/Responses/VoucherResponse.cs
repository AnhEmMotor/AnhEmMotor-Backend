using Domain.Enums;

namespace Application.ApiContracts.Voucher.Responses;

public class VoucherResponse
{
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

    public List<int> AssignedCustomerIds { get; set; } = new();

    public DateTimeOffset? CreatedAt { get; set; }
}
