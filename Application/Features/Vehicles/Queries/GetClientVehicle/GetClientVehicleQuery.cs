using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetClientVehicle;

public sealed record GetClientVehicleQuery(int VehicleId, Guid UserId) : IRequest<Result<VehicleResponse?>>;
