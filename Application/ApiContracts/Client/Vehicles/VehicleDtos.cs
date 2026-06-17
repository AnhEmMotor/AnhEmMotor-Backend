using System;
using System.Collections.Generic;

namespace AnhEmMotor.Application.ApiContracts.Client.Vehicles
{
    public record VehicleSummaryResponse(
        int Id, 
        string ImageUrl, 
        string ModelName, 
        string LicensePlate, 
        double CurrentOdo, 
        int DaysToMaintenance, 
        double KmToMaintenance, 
        string WarrantyQrCode,
        string WarrantyStatus);

    public record VehicleDetailResponse(
        int Id,
        string ModelName,
        string Vin,
        string LicensePlate,
        string TechnicalSpecs,
        List<MaintenanceHistoryDto> History);

    public record MaintenanceHistoryDto(
        DateTime Date, 
        string Description, 
        decimal Cost, 
        string TechnicianName);

    public record UpdateOdoRequest(double NewOdo);
}