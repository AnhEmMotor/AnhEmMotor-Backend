using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Vehicle.Responses;

public class VehicleDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string VinNumber { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public DateTimeOffset PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public double CurrentOdo { get; set; }
    public DateTimeOffset? WarrantyUntil { get; set; }
    public DateTimeOffset? WarrantyFrom { get; set; }
    public DateTimeOffset? InsuranceUntil { get; set; }
    public int? WarrantyRemainingDays { get; set; }
    public object? NextService { get; set; }
    public object? OperatingSpecs { get; set; }
    public List<object> Timeline { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}
