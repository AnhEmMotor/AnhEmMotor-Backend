using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Features.Client.Vehicles.Queries;

public class GetMyVehiclesQueryHandler(IVehicleReadRepository vehicleRepository) : IRequestHandler<GetMyVehiclesQuery, Result<PagedResult<VehicleResponse>>>
{
    public async Task<Result<PagedResult<VehicleResponse>>> Handle(
        GetMyVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = $"-{nameof(Vehicle.PurchaseDate)}";
        }
        Expression<Func<Vehicle, bool>> filter = v => v.UserId == request.UserId;
        var result = await vehicleRepository.GetPagedAsync<VehicleResponse>(
            sieveModel,
            DataFetchMode.ActiveOnly,
            filter,
            cancellationToken)
            .ConfigureAwait(false);

        if (result?.Items != null)
        {
            foreach (var item in result.Items)
            {
                item.CategoryName = GetVehicleType(item.ProductName ?? string.Empty);
            }
        }

        return Result<PagedResult<VehicleResponse>>.Success(result!);
    }

    private string GetVehicleType(string productName)
    {
        var name = productName.ToLower();
        if (name.Contains("sh") || name.Contains("vario") || name.Contains("scooter") || name.Contains("vespa") || name.Contains("vision") || name.Contains("lead") || name.Contains("air blade") || name.Contains("grande"))
            return "Xe ga";
        if (name.Contains("exciter") || name.Contains("winner") || name.Contains("raider") || name.Contains("côn tay"))
            return "Xe côn tay";
        if (name.Contains("z900") || name.Contains("cbr") || name.Contains("ninja") || name.Contains("moto") || name.Contains("phân khối lớn"))
            return "Moto phân khối lớn";
        
        return "Xe số";
    }
}
