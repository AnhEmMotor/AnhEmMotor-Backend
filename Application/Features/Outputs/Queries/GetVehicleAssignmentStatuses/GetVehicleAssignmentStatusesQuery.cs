using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetVehicleAssignmentStatuses;

public sealed record GetVehicleAssignmentStatusesQuery : IRequest<Result<IEnumerable<string>>>;
