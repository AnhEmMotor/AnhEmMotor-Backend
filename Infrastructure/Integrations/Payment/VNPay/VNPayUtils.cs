using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Integrations.Payment.VNPay;

public static class VNPayUtils
{
    public static string HmacSHA512(string key, string InventoryReceiptData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var InventoryReceiptBytes = Encoding.UTF8.GetBytes(InventoryReceiptData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(InventoryReceiptBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }

    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress != null)
            {
                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
                if (remoteIpAddress != null)
                    ipAddress = remoteIpAddress.ToString();
                return ipAddress ?? "127.0.0.1";
            }
        } catch
        {
            return "127.0.0.1";
        }
        return "127.0.0.1";
    }
}
