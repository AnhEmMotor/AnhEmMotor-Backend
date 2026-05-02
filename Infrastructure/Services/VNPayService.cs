using Application.ApiContracts.Payment.Requests;
using Application.ApiContracts.Payment.Responses;
using Application.Interfaces.Services;
using Infrastructure.Integrations.Payment.VNPay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class VNPayService(IConfiguration configuration) : IVNPayService
{
    public string CreatePaymentUrl(HttpContext context, VNPayPaymentRequest model)
    {
        var vnpay = new VnPayLibrary();
        var vnp_TmnCode = configuration["VNPay:TmnCode"];
        var vnp_HashSecret = configuration["VNPay:HashSecret"];
        var vnp_BaseUrl = configuration["VNPay:BaseUrl"];
        var vnp_ReturnUrl = configuration["VNPay:CallbackUrl"];

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode!);
        vnpay.AddRequestData("vnp_Amount", ((long)model.Amount * 100).ToString());
        vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", VNPayUtils.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang:{model.OrderCode}");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl!);
        vnpay.AddRequestData("vnp_TxnRef", model.OrderCode);

        var paymentUrl = vnpay.CreateRequestUrl(vnp_BaseUrl!, vnp_HashSecret!);

        return paymentUrl;
    }

    public VNPayPaymentResponse PaymentExecute(IQueryCollection collections)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if(!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value!);
            }
        }

        var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_SecureHash = collections.FirstOrDefault(
            p => string.Compare(p.Key, "vnp_SecureHash", StringComparison.Ordinal) == 0)
            .Value;
        var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
        var vnp_Amount = vnpay.GetResponseData("vnp_Amount");

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash!, configuration["VNPay:HashSecret"]!);
        if(!checkSignature)
        {
            return new VNPayPaymentResponse { Success = false };
        }

        return new VNPayPaymentResponse
        {
            Success = true,
            PaymentMethod = "VNPay",
            OrderDescription = vnp_OrderInfo,
            OrderId = vnp_orderId,
            TransactionId = vnp_TransactionId,
            Token = vnp_SecureHash!,
            VnPayResponseCode = vnp_ResponseCode,
            Amount = decimal.TryParse(vnp_Amount, out var amount) ? amount / 100 : 0
        };
    }
}
