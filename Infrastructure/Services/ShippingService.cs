using Application.Common.Models;
using Application.Interfaces.Services.Shipping;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class ShippingService(HttpClient httpClient, IConfiguration configuration, ILogger<ShippingService> logger) : IShippingService
{
    public async Task<Result<string>> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var shopId = configuration["GhnSettings:ShopId"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(baseUrl))
            {
                logger.LogWarning("GHN Settings are not configured properly.");
                return Result<string>.Failure(Error.Failure("GHN configuration is missing."));
            }

            var requestUri = $"{baseUrl}/shiip/public-api/v2/shipping-order/create";
            
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);
            request.Headers.Add("ShopId", shopId);

            var products = output.OutputInfos.Select(oi => new
            {
                name = oi.ProductVariant?.Product?.Name ?? "Product",
                code = oi.ProductVariantId?.ToString() ?? oi.Id.ToString(),
                quantity = oi.Count ?? 1,
                price = (int)(oi.Price ?? 0),
                length = 12,
                width = 12,
                height = 12,
                weight = 1200
            }).ToList();

            var payload = new
            {
                payment_type_id = 2,
                note = "Vui lòng gọi trước khi giao",
                required_note = "CHOXEMHANGKHONGTHU",
                client_order_code = $"GHN-{output.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                
                is_new_from_address = true,
                from_name = "Kho Anh Em Motor",
                from_phone = "0987654321", 
                from_address = "Biên Hoà, Đồng Nai",
                from_ward_name = "Phường Trấn Biên",
                from_province_name = "Đồng Nai",
                
                is_new_to_address = true,
                to_name = output.CustomerName ?? "Khách hàng",
                to_phone = output.CustomerPhone ?? "0000000000",
                to_address = output.CustomerAddress ?? "Địa chỉ khách hàng",
                to_ward_name = "Phường Phước Thắng",
                to_province_name = "Hồ Chí Minh",

                cod_amount = (int)(output.Total - (output.PaidAmount ?? 0)),
                weight = 1200,
                length = 12,
                width = 12,
                height = 12,
                insurance_value = (int)output.Total,
                service_type_id = 2,
                items = products
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            request.Content = JsonContent.Create(payload, null, jsonOptions);

            var payloadString = JsonSerializer.Serialize(payload, jsonOptions);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure(Error.BadRequest("Failed to create shipping order with GHN: " + contentString));
            }

            using var jsonDocument = JsonDocument.Parse(contentString);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Unknown error";
                return Result<string>.Failure(Error.BadRequest("GHN Error: " + message));
            }

            var orderCode = "Unknown";
            if (root.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("order_code", out var orderCodeElement))
            {
                orderCode = orderCodeElement.GetString() ?? "Unknown";
            }

            return Result<string>.Success(orderCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating shipping order for OutputId: {OutputId}", output.Id);
            return Result<string>.Failure(Error.Failure("An error occurred while creating the shipping order."));
        }
    }

    public async Task<Result<string>> GetShippingOrderStatusAsync(string orderCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<string>.Failure(Error.Failure("GHN configuration is missing."));
            }

            var requestUri = $"{baseUrl}/shiip/public-api/v2/shipping-order/detail";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);

            var payload = new { order_code = orderCode };
            request.Content = JsonContent.Create(payload);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure(Error.BadRequest("Failed to get order status from GHN: " + contentString));
            }

            using var jsonDocument = JsonDocument.Parse(contentString);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Unknown error";
                return Result<string>.Failure(Error.BadRequest("GHN Error: " + message));
            }

            if (root.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("status", out var statusElement))
            {
                return Result<string>.Success(statusElement.GetString() ?? "unknown");
            }

            return Result<string>.Failure(Error.Failure("Cannot parse status from GHN response."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting order status for GHN OrderCode: {OrderCode}", orderCode);
            return Result<string>.Failure(Error.Failure("An error occurred while getting order status."));
        }
    }

    public async Task<Result<object>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<object>.Failure(Error.Failure("GHN configuration is missing."));
            }

            var requestUri = $"{baseUrl}/shiip/public-api/v3/master-data/province/all";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);

            var payload = new { offset = 0, limit = 100 };
            request.Content = JsonContent.Create(payload);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<object>.Failure(Error.BadRequest("Failed to fetch provinces: " + contentString));
            }

            var jsonObject = JsonSerializer.Deserialize<object>(contentString);
            return Result<object>.Success(jsonObject!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching provinces from GHN");
            return Result<object>.Failure(Error.Failure("An error occurred while fetching provinces."));
        }
    }

    public async Task<Result<object>> GetWardsAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<object>.Failure(Error.Failure("GHN configuration is missing."));
            }

            var requestUri = $"{baseUrl}/shiip/public-api/v3/master-data/ward/all-by-province-id";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);

            var payload = new { province_id = provinceId, offset = 0, limit = 200 };
            request.Content = JsonContent.Create(payload);

            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<object>.Failure(Error.BadRequest("Failed to fetch wards: " + contentString));
            }

            var jsonObject = JsonSerializer.Deserialize<object>(contentString);
            return Result<object>.Success(jsonObject!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching wards from GHN");
            return Result<object>.Failure(Error.Failure("An error occurred while fetching wards."));
        }
    }
}
