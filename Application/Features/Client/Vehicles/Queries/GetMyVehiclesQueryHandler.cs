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
            
        return result;
    }
}
