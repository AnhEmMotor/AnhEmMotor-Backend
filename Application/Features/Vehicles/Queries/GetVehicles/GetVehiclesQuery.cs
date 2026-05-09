using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Vehicles.Queries.GetVehicles;

public sealed record GetVehiclesQuery : IRequest<Result<PagedResult<VehicleResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

