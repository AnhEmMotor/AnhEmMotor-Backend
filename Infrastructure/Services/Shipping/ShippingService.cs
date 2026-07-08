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

namespace Infrastructure.Services.Shipping;

public class ShippingService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(HttpClient httpClient, IConfiguration configuration, ILogger<ShippingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> CreateShippingOrderAsync(Output output, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = _configuration["GhtkSettings:Token"];
            var shopId = _configuration["GhtkSettings:ShopId"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(shopId))
            {
                _logger.LogWarning("GHTK Settings are not configured properly.");
                return Result.Failure(Error.Failure("GHTK configuration is missing."));
            }

            var requestUri = "/services/shipment/order/?ver=1.5";
            
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);
            request.Headers.Add("X-Client-Source", shopId);

            var products = output.OutputInfos.Select(oi => new
            {
                name = oi.ProductVariant?.Product?.Name ?? "Product",
                weight = 0.1, // Fixed weight as requested
                quantity = oi.Count ?? 1,
                price = (int)(oi.Price ?? 0),
                product_code = oi.ProductVariantId ?? oi.Id
            }).ToList();

            var payload = new
            {
                products = products,
                order = new
                {
                    id = $"{output.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                    pick_name = "Kho Anh Em Motor", // Example
                    pick_address = "123 Đường XYZ",
                    pick_province = "TP. Hồ Chí Minh",
                    pick_district = "Quận 1",
                    pick_ward = "Phường Bến Nghé",
                    pick_tel = "0123456789",
                    
                    tel = output.CustomerPhone ?? "0000000000",
                    name = output.CustomerName ?? "Khách hàng",
                    address = output.CustomerAddress ?? "Địa chỉ khách hàng",
                    province = "TP. Hồ Chí Minh", // Fallbacks
                    district = "Quận 1",
                    ward = "Phường Bến Nghé",
                    hamlet = "Khác",
                    
                    is_freeship = "0", // As requested: người nhận trả
                    pick_money = (int)(output.Total - (output.PaidAmount ?? 0)), // Cod amount
                    value = (int)output.Total, // Value for insurance
                    transport = "fly",
                    pick_option = "cod"
                }
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            request.Content = JsonContent.Create(payload, null, jsonOptions);

            var payloadString = JsonSerializer.Serialize(payload, jsonOptions);
            _logger.LogInformation("GHTK Request Payload: {Payload}", payloadString);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("GHTK Response: {Response}", contentString);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(Error.BadRequest("Failed to create shipping order with GHTK: " + contentString));
            }

            using var jsonDocument = JsonDocument.Parse(contentString);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("success", out var successElement) && !successElement.GetBoolean())
            {
                var message = root.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Unknown error";
                return Result.Failure(Error.BadRequest("GHTK Error: " + message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping order for OutputId: {OutputId}", output.Id);
            return Result.Failure(Error.Failure("An error occurred while creating the shipping order."));
        }
    }
}
