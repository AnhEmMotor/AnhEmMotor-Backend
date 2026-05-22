namespace Application.ApiContracts.Output.Responses;

public sealed class VehicleAssignmentRequirementResponse
{
    public int OrderId { get; set; }

    public string TargetStatusId { get; set; } = string.Empty;

    public bool RequiresVehicleAssignment { get; set; }

    public List<VehicleAssignmentRequirementItemResponse> Items { get; set; } = [];
}

public sealed class VehicleAssignmentRequirementItemResponse
{
    public int OutputInfoId { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? ProductVariantId { get; set; }

    public string? ProductVariantName { get; set; }

    public int? ProductVariantColorId { get; set; }

    public string? ColorName { get; set; }

    public int RequiredCount { get; set; }

    public List<VehicleAssignmentOptionResponse> AssignedVehicles { get; set; } = [];

    public List<VehicleAssignmentOptionResponse> AvailableVehicles { get; set; } = [];

    public int AvailableCount { get; set; }

    public bool CanFulfill { get; set; }
}

public sealed class VehicleAssignmentOptionResponse
{
    public int Id { get; set; }

    public string VinNumber { get; set; } = string.Empty;

    public string EngineNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
