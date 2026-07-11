using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Commands.DeleteClientVehicle;

public sealed record DeleteClientVehicleCommand(int VehicleId, Guid UserId) : IRequest<Result>;
