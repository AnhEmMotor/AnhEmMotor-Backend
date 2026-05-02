using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;

    public VNPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(HttpContext context, VNPayPaymentRequest model)
    {
        var tick = DateTime.Now.Ticks.ToString();
        var vnpay = new VnPayLibrary();
        var vnp_TmnCode = _configuration["VNPay:TmnCode"];
        var vnp_HashSecret = _configuration["VNPay:HashSecret"];
        var vnp_BaseUrl = _configuration["VNPay:BaseUrl"];
        var vnp_ReturnUrl = _configuration["VNPay:CallbackUrl"];

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode!);
        vnpay.AddRequestData("vnp_Amount", ((long)model.Amount * 100).ToString());
        vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + model.OrderCode);
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
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value!);
            }
        }

        var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
        var vnp_Amount = vnpay.GetResponseData("vnp_Amount");

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash!, _configuration["VNPay:HashSecret"]!);
        if (!checkSignature)
        {
            return new VNPayPaymentResponse
            {
                Success = false
            };
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

public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var ret) ? ret : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        var queryString = data.ToString();

        baseUrl += "?" + queryString;
        var signData = queryString;
        if (signData.Length > 0)
        {
            signData = signData.Remove(data.Length - 1);
        }

        var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
        baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseRaw();
        var myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRaw()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

public static class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = string.Empty;
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress != null)
            {
                if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                }

                if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                return ipAddress;
            }
        }
        catch
        {
            return "127.0.0.1";
        }

        return "127.0.0.1";
    }
}
