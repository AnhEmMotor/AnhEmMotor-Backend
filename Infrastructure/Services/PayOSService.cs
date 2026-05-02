using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services;

public class PayOSService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IPayOSService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<PayOSPaymentResponse> CreatePaymentAsync(PayOSPaymentRequest request)
    {
        var clientId = _configuration["PayOS:ClientId"];
        var apiKey = _configuration["PayOS:ApiKey"];
        var checksumKey = _configuration["PayOS:ChecksumKey"];
        var baseUrl = _configuration["PayOS:BaseUrl"]?.TrimEnd('/');
        var returnUrl = _configuration["PayOS:ReturnUrl"];
        var cancelUrl = _configuration["PayOS:CancelUrl"];

        var payload = new
        {
            orderCode = request.OrderCode,
            amount = (long)request.Amount,
            description = request.Description,
            cancelUrl = cancelUrl,
            returnUrl = returnUrl,
            signature = ""
        };

        // Create signature
        // amount=...&cancelUrl=...&description=...&orderCode=...&returnUrl=...
        var signatureData = $"amount={payload.amount}&cancelUrl={payload.cancelUrl}&description={payload.description}&orderCode={payload.orderCode}&returnUrl={payload.returnUrl}";
        var signature = CreateSignature(signatureData, checksumKey!);

        var finalPayload = new
        {
            payload.orderCode,
            payload.amount,
            payload.description,
            payload.cancelUrl,
            payload.returnUrl,
            signature = signature
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-client-id", clientId);
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var response = await client.PostAsJsonAsync($"{baseUrl}/v2/payment-requests", finalPayload);
        var content = await response.Content.ReadAsStringAsync();
        
        var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payosResponse?.Code == "00")
        {
            return new PayOSPaymentResponse
            {
                ErrorCode = 0,
                CheckoutUrl = payosResponse.Data.CheckoutUrl,
                PaymentLinkId = payosResponse.Data.PaymentLinkId
            };
        }

        return new PayOSPaymentResponse
        {
            ErrorCode = 1,
            Message = payosResponse?.Desc ?? "Error calling PayOS"
        };
    }

    public bool VerifyWebhook(PayOSWebhookData data)
    {
        // For simplicity, we can trust the signature if we verify it correctly.
        // PayOS webhook signature verification:
        // amount=...&description=...&orderCode=...&transactionId=...
        var checksumKey = _configuration["PayOS:ChecksumKey"];
        var signatureData = $"amount={data.Amount}&description={data.Description}&orderCode={data.OrderCode}&transactionId={data.TransactionId}";
        var expectedSignature = CreateSignature(signatureData, checksumKey!);
        
        return expectedSignature == data.Signature;
    }

    public async Task<Application.Interfaces.Services.PayOSData?> GetPaymentDetailsAsync(long orderCode)
    {
        var clientId = _configuration["PayOS:ClientId"];
        var apiKey = _configuration["PayOS:ApiKey"];
        var baseUrl = _configuration["PayOS:BaseUrl"]?.TrimEnd('/');

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-client-id", clientId);
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var response = await client.GetAsync($"{baseUrl}/v2/payment-requests/{orderCode}");
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payosResponse?.Code == "00" && payosResponse.Data != null)
        {
            return new Application.Interfaces.Services.PayOSData
            {
                OrderCode = payosResponse.Data.OrderCode,
                Amount = payosResponse.Data.Amount,
                Description = payosResponse.Data.Description,
                Status = payosResponse.Data.Status,
                CheckoutUrl = payosResponse.Data.CheckoutUrl,
                PaymentLinkId = payosResponse.Data.PaymentLinkId
            };
        }

        return null;
    }

    private string CreateSignature(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

public class PayOSApiResponse
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public PayOSInternalData Data { get; set; } = new();
}

public class PayOSInternalData
{
    [JsonPropertyName("bin")]
    public string Bin { get; set; } = string.Empty;
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;
    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    [JsonPropertyName("paymentLinkId")]
    public string PaymentLinkId { get; set; } = string.Empty;
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("checkoutUrl")]
    public string CheckoutUrl { get; set; } = string.Empty;
    [JsonPropertyName("qrCode")]
    public string QrCode { get; set; } = string.Empty;
}
