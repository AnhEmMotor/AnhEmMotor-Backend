using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Voucher.Requests;

/// <summary>
/// Request body for validating a voucher before applying it to an order.
/// </summary>
public class VoucherValidateRequest
{
    [Required]
    public int VoucherId { get; set; }

    public int? OutputId { get; set; }

    public decimal? OrderTotal { get; set; }
}
