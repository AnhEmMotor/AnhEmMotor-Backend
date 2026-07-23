using Application.Common.Models;
using Application.Interfaces.Services.Shipping;
using MediatR;
using System.Text.Json;

namespace Application.Features.Outputs.Queries.GetProvinces;

public sealed class GetProvincesQueryHandler(IShippingService shippingService) : IRequestHandler<GetProvincesQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetProvincesQuery request, CancellationToken cancellationToken)
    {
        var result = await shippingService.GetProvincesAsync(cancellationToken);
        if (!result.IsSuccess || string.IsNullOrEmpty(result.Value))
        {
            return Result<object>.Failure(result.Error ?? Error.Failure("Unknown error."));
        }
        try
        {
            using var document = JsonDocument.Parse(result.Value);
            var root = document.RootElement;
            if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                var provinces = dataElement.EnumerateArray()
                    .Select(
                        p => new
                        {
                            ProvinceId = p.TryGetProperty("_id", out var idProp) ? idProp.GetInt32() : 0,
                            ProvinceName = p.TryGetProperty("name", out var nameProp)
                                ? nameProp.GetString()
                                : string.Empty
                        })
                    .ToList();
                return Result<object>.Success(provinces);
            }
            return Result<object>.Failure(Error.Failure("Invalid data format from GHN API."));
        } catch (JsonException)
        {
            return Result<object>.Failure(Error.Failure("Failed to parse GHN API response."));
        }
    }
}
