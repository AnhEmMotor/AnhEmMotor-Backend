using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehicleHistory;

public sealed record GetVehicleHistoryQuery(int VehicleId) : IRequest<Result<VehicleHistoryResponse>>;
