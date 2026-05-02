using Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IVNPayService
{
    string CreatePaymentUrl(HttpContext context, VNPayPaymentRequest model);
    VNPayPaymentResponse PaymentExecute(IQueryCollection collections);
}

public class VNPayPaymentRequest
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class VNPayPaymentResponse
{
    public bool Success { get; set; }
    public string PaymentMethod { get; set; } = "VNPay";
    public string OrderDescription { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string VnPayResponseCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
