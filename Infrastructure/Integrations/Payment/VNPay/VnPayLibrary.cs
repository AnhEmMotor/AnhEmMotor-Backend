using System.Net;
using System.Text;

namespace Infrastructure.Integrations.Payment.VNPay;

public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if(!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if(!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    { return _responseData.TryGetValue(key, out var ret) ? ret : string.Empty; }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach(var kv in _requestData)
        {
            if(!string.IsNullOrEmpty(kv.Value))
            {
                data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
            }
        }

        var queryString = data.ToString();

        baseUrl = $"{baseUrl}?{queryString}";
        var signData = queryString;
        if(signData.Length > 0)
        {
            signData = signData.Remove(data.Length - 1);
        }

        var vnp_SecureHash = VNPayUtils.HmacSHA512(vnp_HashSecret, signData);
        baseUrl = $"{baseUrl}vnp_SecureHash={vnp_SecureHash}";

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseRaw();
        var myChecksum = VNPayUtils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRaw()
    {
        var data = new StringBuilder();
        if(_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if(_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach(var kv in _responseData)
        {
            if(!string.IsNullOrEmpty(kv.Value))
            {
                data.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
            }
        }

        if(data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
}
