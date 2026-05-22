using System;

namespace Application.ApiContracts.Output.Responses;

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