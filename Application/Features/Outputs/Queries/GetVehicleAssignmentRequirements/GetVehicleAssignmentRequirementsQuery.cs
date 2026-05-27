using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetVehicleAssignmentRequirements;

public sealed record GetVehicleAssignmentRequirementsQuery : IRequest<Result<VehicleAssignmentRequirementResponse>>
{
    public int Id { get; init; }

    public string TargetStatusId { get; init; } = string.Empty;
}
