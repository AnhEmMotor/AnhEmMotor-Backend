using Application.Common.Models;
using Application.Interfaces.Services.Shipping;
using Application.Interfaces.Services.Shipping.Models;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services;

public class ShippingService(HttpClient httpClient, IConfiguration configuration) : IShippingService
{
    public async Task<Result<decimal>> CalculateShippingFeeAsync(
        CalculateShippingFeeRequest req,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var shopId = configuration["GhnSettings:ShopId"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<decimal>.Failure(Error.Failure("GHN configuration is missing."));
            }
            var requestUri = $"{baseUrl}/shiip/public-api/v2/shipping-order/fee";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);
            request.Headers.Add("ShopId", shopId);
            var products = req.Items
                .Select(
                    i => new
                    {
                        name = i.Name,
                        quantity = i.Quantity,
                        length = Math.Min(150, Math.Max(1, i.Length ?? 12)),
                        width = Math.Min(150, Math.Max(1, i.Width ?? 12)),
                        height = Math.Min(150, Math.Max(1, i.Height ?? 12)),
                        weight = Math.Min(30000, Math.Max(1, i.Weight ?? 1200))
                    })
                .ToList();
            var totalWeight = Math.Min(30000, products.Sum(x => x.weight * x.quantity));
            var maxLength = products.Any() ? Math.Min(150, products.Max(x => x.length)) : 12;
            var maxWidth = products.Any() ? Math.Min(150, products.Max(x => x.width)) : 12;
            var totalHeight = Math.Min(150, products.Sum(x => x.height * x.quantity));
            var payload = new
            {
                to_ward_id_v2 = req.ToWardIdV2,
                to_address_v2 = req.ToAddressV2,
                is_new_to_address = req.IsNewToAddress,
                to_ward_code = req.ToWardCode,
                service_type_id = 5,
                weight = totalWeight,
                length = maxLength,
                width = maxWidth,
                height = totalHeight,
                items = products
            };
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            request.Content = JsonContent.Create(payload, null, jsonOptions);
            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<decimal>.Failure(
                    Error.BadRequest("Failed to calculate shipping fee with GHN: " + contentString));
            }
            using var jsonDocument = JsonDocument.Parse(contentString);
            var root = jsonDocument.RootElement;
            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : "Unknown error";
                return Result<decimal>.Failure(Error.BadRequest("GHN Error: " + message));
            }
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("total", out var totalElement))
            {
                return Result<decimal>.Success(totalElement.GetDecimal());
            }
            return Result<decimal>.Failure(Error.Failure("Cannot parse fee from GHN response."));
        } catch (Exception ex)
        {
            return Result<decimal>.Failure(
                Error.Failure("An error occurred while calculating shipping fee. " + ex.Message));
        }
    }

    public async Task<Result<string>> CreateShippingOrderAsync(
        Output output,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var shopId = configuration["GhnSettings:ShopId"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<string>.Failure(Error.Failure("GHN configuration is missing."));
            }
            var requestUri = $"{baseUrl}/shiip/public-api/v2/shipping-order/create";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Token", token);
            request.Headers.Add("ShopId", shopId);
            var products = output.OutputInfos
                .Select(
                    oi =>
                    {
                        var v = oi.ProductVariant;
                        var p = v?.Product;
                        var length = v?.Length ?? p?.Length ?? 12;
                        var width = v?.Width ?? p?.Width ?? 12;
                        var height = v?.Height ?? p?.Height ?? 12;
                        var weight = v?.Weight ?? p?.Weight ?? 1.2m;
                        return new
                        {
                            name = p?.Name ?? "Product",
                            code = oi.ProductVariantId?.ToString() ?? oi.Id.ToString(),
                            quantity = oi.Count ?? 1,
                            price = (int)(oi.Price ?? 0),
                            length = Math.Min(150, Math.Max(1, (int)length)),
                            width = Math.Min(150, Math.Max(1, (int)width)),
                            height = Math.Min(150, Math.Max(1, (int)height)),
                            weight = Math.Min(30000, Math.Max(1, (int)(weight * 1000)))
                        };
                    })
                .ToList();
            var totalWeight = Math.Min(30000, products.Sum(x => x.weight * x.quantity));
            var maxLength = products.Any() ? Math.Min(150, products.Max(x => x.length)) : 12;
            var maxWidth = products.Any() ? Math.Min(150, products.Max(x => x.width)) : 12;
            var totalHeight = Math.Min(150, products.Sum(x => x.height * x.quantity));
            var payload = new
            {
                payment_type_id = 2,
                note = output.Notes ?? string.Empty,
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
                to_ward_name = !string.IsNullOrWhiteSpace(output.WardName) ? output.WardName : "Phường Phước Thắng",
                to_province_name = !string.IsNullOrWhiteSpace(output.ProvinceName) ? output.ProvinceName : "Hồ Chí Minh",
                cod_amount = (int)(output.Total - (output.PaidAmount ?? 0)),
                weight = totalWeight,
                length = maxLength,
                width = maxWidth,
                height = totalHeight,
                insurance_value = (int)output.Total,
                service_type_id = 5,
                items = products
            };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            request.Content = JsonContent.Create(payload, null, jsonOptions);
            var response = await httpClient.SendAsync(request, cancellationToken);
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure(
                    Error.BadRequest("Failed to create shipping order with GHN: " + contentString));
            }
            using var jsonDocument = JsonDocument.Parse(contentString);
            var root = jsonDocument.RootElement;
            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : "Unknown error";
                return Result<string>.Failure(Error.BadRequest("GHN Error: " + message));
            }
            var orderCode = "Unknown";
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("order_code", out var orderCodeElement))
            {
                orderCode = orderCodeElement.GetString() ?? "Unknown";
            }
            return Result<string>.Success(orderCode);
        } catch (Exception)
        {
            return Result<string>.Failure(Error.Failure("An error occurred while creating the shipping order."));
        }
    }

    public async Task<Result<string>> GetShippingOrderStatusAsync(
        string orderCode,
        CancellationToken cancellationToken = default)
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
                var message = root.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : "Unknown error";
                return Result<string>.Failure(Error.BadRequest("GHN Error: " + message));
            }
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("status", out var statusElement))
            {
                return Result<string>.Success(statusElement.GetString() ?? "unknown");
            }
            return Result<string>.Failure(Error.Failure("Cannot parse status from GHN response."));
        } catch (Exception)
        {
            return Result<string>.Failure(Error.Failure("An error occurred while getting order status."));
        }
    }

    public async Task<Result<string>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<string>.Failure(
                    Error.ServiceUnavailable("GHN configuration is missing. Configure GhnSettings:Token and GhnSettings:BaseUrl."));
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
                return Result<string>.Failure(
                    Error.ServiceUnavailable("GHN could not provide provinces. Upstream response: " + contentString));
            }
            return Result<string>.Success(contentString);
        } catch (Exception ex)
        {
            return Result<string>.Failure(
                Error.ServiceUnavailable("GHN province service is unavailable: " + ex.Message));
        }
    }

    public async Task<Result<string>> GetWardsAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = configuration["GhnSettings:Token"];
            var baseUrl = configuration["GhnSettings:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl))
            {
                return Result<string>.Failure(Error.Failure("GHN configuration is missing."));
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
                return Result<string>.Failure(Error.BadRequest("Failed to fetch wards: " + contentString));
            }
            return Result<string>.Success(contentString);
        } catch (Exception)
        {
            return Result<string>.Failure(Error.Failure("An error occurred while fetching wards."));
        }
    }

    public async Task<string?> GetProvinceNameAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        var result = await GetProvincesAsync(cancellationToken);
        if (!result.IsSuccess || string.IsNullOrEmpty(result.Value))
            return null;
        try
        {
            using var document = JsonDocument.Parse(result.Value);
            if (document.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in dataElement.EnumerateArray())
                {
                    if (p.TryGetProperty("_id", out var idProp) && idProp.GetInt32() == provinceId)
                    {
                        return p.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    }
                }
            }
        } catch
        {
        }
        return null;
    }

    public async Task<string?> GetWardNameAsync(
        int provinceId,
        string wardCode,
        CancellationToken cancellationToken = default)
    {
        var result = await GetWardsAsync(provinceId, cancellationToken);
        if (!result.IsSuccess || string.IsNullOrEmpty(result.Value))
            return null;
        try
        {
            using var document = JsonDocument.Parse(result.Value);
            if (document.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var w in dataElement.EnumerateArray())
                {
                    if (w.TryGetProperty("_id", out var codeProp) && codeProp.GetInt32().ToString() == wardCode)
                    {
                        return w.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    }
                }
            }
        } catch
        {
        }
        return null;
    }
}

