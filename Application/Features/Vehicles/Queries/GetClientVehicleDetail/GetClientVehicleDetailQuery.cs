using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetClientVehicleDetail;

public sealed record GetClientVehicleDetailQuery(int VehicleId, Guid UserId) : IRequest<Result<VehicleResponse?>>;
