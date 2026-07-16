using Application.Common.Models;
using Application.Interfaces.Services.Shipping;
using MediatR;
using System.Text.Json;

namespace Application.Features.Outputs.Queries.GetWards;

public sealed class GetWardsQueryHandler(IShippingService shippingService) : IRequestHandler<GetWardsQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetWardsQuery request, CancellationToken cancellationToken)
    {
        var result = await shippingService.GetWardsAsync(request.ProvinceId, cancellationToken);
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
                var wards = dataElement.EnumerateArray()
                    .Select(
                        w => new
                        {
                            WardCode = w.TryGetProperty("_id", out var codeProp)
                                ? codeProp.GetInt32().ToString()
                                : string.Empty,
                            WardName = w.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : string.Empty
                        })
                    .ToList();
                return Result<object>.Success(wards);
            }
            return Result<object>.Failure(Error.Failure("Invalid data format from GHN API."));
        } catch (JsonException)
        {
            return Result<object>.Failure(Error.Failure("Failed to parse GHN API response."));
        }
    }
}
