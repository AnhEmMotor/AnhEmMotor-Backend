using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories;
using MediatR;
using System;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleDetail;

public class GetCustomerVehicleDetailQueryHandler(
    IVehicleReadRepository vehicleRepository,
    IMaintenanceHistoryReadRepository maintenanceRepository) : IRequestHandler<GetCustomerVehicleDetailQuery, Result<VehicleDetailResponse>>
{
    public async Task<Result<VehicleDetailResponse>> Handle(
        GetCustomerVehicleDetailQuery request,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle == null || vehicle.UserId != request.UserId)
        {
            return Result<VehicleDetailResponse>.Failure(Error.NotFound("Không tìm thấy thông tin xe.", "VehicleId"));
        }
        var response = new VehicleDetailResponse
        {
            Id = vehicle.Id,
            Name = vehicle.Product?.Name ?? vehicle.Lead?.FullName ?? "Xe của tôi",
            LicensePlate = vehicle.LicensePlate,
            VinNumber = vehicle.VinNumber,
            EngineNumber = vehicle.EngineNumber,
            ColorName = vehicle.ProductVariantColor?.ColorName ?? string.Empty,
            Type = GetVehicleType(vehicle.Product?.Name ?? string.Empty),
            VariantName = vehicle.ProductVariant?.VariantName ?? string.Empty,
            Capacity = vehicle.Product?.Displacement?.ToString() ?? "",
            PurchaseDate = vehicle.PurchaseDate,
            Status = vehicle.Status,
            CurrentOdo = vehicle.CurrentOdo,
            WarrantyFrom = vehicle.PurchaseDate,
            WarrantyUntil = vehicle.PurchaseDate.AddMonths(GetMonths(vehicle.Product?.WarrantyPeriod)),
            ImageUrl = vehicle.ProductVariantColor?.CoverImageUrl ?? string.Empty,
            OperatingSpecs = new
            {
                oil = vehicle.Product?.OilCapacity > 0 ? $"10W-30 Full Synthetic - {vehicle.Product.OilCapacity} L" : "10W-30 Full Synthetic",
                tirePressure = "2.0 bar (Trước) / 2.25 bar (Sau)"
            }
        };
        if (response.WarrantyUntil.HasValue)
        {
            response.WarrantyRemainingDays = (int)(response.WarrantyUntil.Value - DateTimeOffset.UtcNow).TotalDays;
            if (response.WarrantyRemainingDays < 0)
                response.WarrantyRemainingDays = 0;
        }

        var histories = await maintenanceRepository.GetByVehicleIdAsync(vehicle.Id, cancellationToken);
        var orderedHistories = histories.OrderByDescending(h => h.MaintenanceDate).ToList();

        response.Timeline = orderedHistories.Select(h => (object)new
        {
            id = h.Id.ToString(),
            date = h.MaintenanceDate.ToString("yyyy-MM-dd"),
            title = h.ServiceType ?? "Bảo dưỡng",
            items = new[] { $"Số km: {h.Mileage} km", $"Chi phí: {h.TotalCost} đ", h.Description },
            status = "completed"
        }).ToList();

        var lastMaintenance = orderedHistories.FirstOrDefault();
        if (lastMaintenance != null)
        {
            response.NextService = new
            {
                odo = lastMaintenance.NextMaintenanceOdo?.ToString() ?? (lastMaintenance.Mileage + 3000).ToString(),
                date = lastMaintenance.NextMaintenanceDate?.ToString("yyyy-MM-dd") ?? lastMaintenance.MaintenanceDate.AddMonths(3).ToString("yyyy-MM-dd"),
                items = new[] { "Thay nhớt định kỳ", "Kiểm tra phanh" }
            };
        }
        else
        {
            response.NextService = new
            {
                odo = (vehicle.CurrentOdo + 1000).ToString(),
                date = DateTimeOffset.UtcNow.AddMonths(1).ToString("yyyy-MM-dd"),
                items = new[] { "Kiểm tra tổng quát lần 1" }
            };
        }

        return Result<VehicleDetailResponse>.Success(response);
    }

    private int GetMonths(string? warrantyPeriod)
    {
        if (string.IsNullOrWhiteSpace(warrantyPeriod))
            return 36;
        if (warrantyPeriod.Contains("36"))
            return 36;
        if (warrantyPeriod.Contains("24"))
            return 24;
        if (warrantyPeriod.Contains("12"))
            return 12;
        return 36;
    }

    private string GetVehicleType(string productName)
    {
        var name = productName.ToLower();
        if (name.Contains("sh") || name.Contains("vario") || name.Contains("scooter") || name.Contains("vespa") || name.Contains("vision") || name.Contains("lead") || name.Contains("air blade") || name.Contains("grande"))
            return "Xe ga";
        if (name.Contains("exciter") || name.Contains("winner") || name.Contains("raider") || name.Contains("côn tay"))
            return "Xe côn tay";
        if (name.Contains("z900") || name.Contains("cbr") || name.Contains("ninja") || name.Contains("moto") || name.Contains("phân khối lớn"))
            return "Moto phân khối lớn";
        
        return "Xe số";
    }
}
