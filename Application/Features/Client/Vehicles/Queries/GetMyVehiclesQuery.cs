using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Client.Vehicles.Queries;

public class GetMyVehiclesQuery : IRequest<Result<PagedResult<VehicleResponse>>>
{
    public Guid UserId { get; set; }

    public SieveModel? SieveModel { get; set; }
}
