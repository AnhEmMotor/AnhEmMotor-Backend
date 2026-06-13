using System;

namespace Application.ApiContracts.Output.Responses;

public class VehicleAssignmentOptionResponse
{
    public int Id { get; set; }

    public string VinNumber { get; set; } = string.Empty;

    public string EngineNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
