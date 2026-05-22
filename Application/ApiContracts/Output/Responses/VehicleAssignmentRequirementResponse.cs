
namespace Application.ApiContracts.Output.Responses;

public sealed class VehicleAssignmentRequirementResponse
{
    public int OrderId { get; set; }

    public string TargetStatusId { get; set; } = string.Empty;

    public bool RequiresVehicleAssignment { get; set; }

    public List<VehicleAssignmentRequirementItemResponse> Items { get; set; } = [];
}

