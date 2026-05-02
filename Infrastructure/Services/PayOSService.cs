using Application.ApiContracts.Payment.Requests;
using Application.ApiContracts.Payment.Responses;
using Application.Interfaces.Services;
using Domain.Constants.Order;
using Infrastructure.Integrations.Payment.PayOS;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

public class PayOSService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IPayOSService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<PayOSPaymentResponse> CreatePaymentAsync(
        PayOSPaymentRequest request,
        CancellationToken cancellationToken)
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
            cancelUrl,
            returnUrl,
            signature = string.Empty
        };

        var signatureData = $"amount={payload.amount}&cancelUrl={payload.cancelUrl}&description={payload.description}&orderCode={payload.orderCode}&returnUrl={payload.returnUrl}";
        var signature = CreateSignature(signatureData, checksumKey!);

        var finalPayload = new
        {
            payload.orderCode,
            payload.amount,
            payload.description,
            payload.cancelUrl,
            payload.returnUrl,
            signature
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-client-id", clientId);
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var response = await client.PostAsJsonAsync($"{baseUrl}/v2/payment-requests", finalPayload, cancellationToken)
            .ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(content, _jsonOptions);

        if(payosResponse != null &&
            string.Compare(payosResponse.Code, PayOSStatus.SuccessCode) == 0 &&
            payosResponse.Data != null)
        {
            return new PayOSPaymentResponse
            {
                ErrorCode = 0,
                CheckoutUrl = payosResponse.Data.CheckoutUrl,
                PaymentLinkId = payosResponse.Data.PaymentLinkId
            };
        }

        return new PayOSPaymentResponse { ErrorCode = 1, Message = payosResponse?.Desc ?? "Error calling PayOS" };
    }

    public bool VerifyWebhook(PayOSWebhookData data)
    {
        var checksumKey = _configuration["PayOS:ChecksumKey"];
        var signatureData = $"amount={data.Amount}&description={data.Description}&orderCode={data.OrderCode}&transactionId={data.TransactionId}";
        var expectedSignature = CreateSignature(signatureData, checksumKey!);

        return string.Compare(expectedSignature, data.Signature) == 0;
    }

    public async Task<PayOSData?> GetPaymentDetailsAsync(long orderCode, CancellationToken cancellationToken)
    {
        var clientId = _configuration["PayOS:ClientId"];
        var apiKey = _configuration["PayOS:ApiKey"];
        var baseUrl = _configuration["PayOS:BaseUrl"]?.TrimEnd('/');

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-client-id", clientId);
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var response = await client.GetAsync($"{baseUrl}/v2/payment-requests/{orderCode}", cancellationToken)
            .ConfigureAwait(false);
        if(!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(content, _jsonOptions);

        if(payosResponse != null &&
            string.Compare(payosResponse.Code, PayOSStatus.SuccessCode) == 0 &&
            payosResponse.Data != null)
        {
            return new PayOSData
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

    private static string CreateSignature(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
