using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Voucher.Requests;

public class CreateVoucherRequest
{
    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public VoucherApplyFor ApplyFor { get; set; }

    public VoucherChannel Channel { get; set; }

    public VoucherType Type { get; set; }

    public DiscountType DiscountType { get; set; }

    [Required]
    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }

    public int UsageLimitPerUser { get; set; } = 1;

    public int TotalUsageLimit { get; set; } = 0;

    public List<int> AssignedCustomerIds { get; set; } = new();
}
