using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehicles;

public sealed class GetVehiclesQuery : IRequest<Result<List<VehicleResponse>>>
{
    public string? Search { get; set; }
}

