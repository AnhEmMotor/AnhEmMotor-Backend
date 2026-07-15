namespace Application.ApiContracts.Voucher.Responses;

/// <summary>
/// Response returned by the ValidateVoucher endpoint.
/// </summary>
public class VoucherValidateResponse
{
    /// <summary>
    /// true if the voucher is valid and can be applied.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Human-readable message explaining the result.
    /// Empty when valid.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Calculated discount amount that would be applied.
    /// </summary>
    public decimal DiscountAmount { get; set; }
}
