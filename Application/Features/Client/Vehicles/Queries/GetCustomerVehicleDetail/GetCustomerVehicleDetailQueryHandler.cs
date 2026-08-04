using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using System;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleDetail;

public class GetCustomerVehicleDetailQueryHandler(IVehicleReadRepository vehicleRepository) : IRequestHandler<GetCustomerVehicleDetailQuery, Result<VehicleDetailResponse>>
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
            Type = vehicle.Product?.ProductCategory?.Name ?? "Xe máy",
            VariantName = vehicle.ProductVariant?.VariantName ?? string.Empty,
            Capacity = string.Empty,
            PurchaseDate = vehicle.PurchaseDate,
            Status = vehicle.Status,
            CurrentOdo = vehicle.CurrentOdo,
            WarrantyFrom = vehicle.PurchaseDate,
            WarrantyUntil = vehicle.PurchaseDate.AddMonths(GetMonths(vehicle.Product?.WarrantyPeriod)),
            ImageUrl = vehicle.ProductVariantColor?.CoverImageUrl ?? string.Empty
        };
        if (response.WarrantyUntil.HasValue)
        {
            response.WarrantyRemainingDays = (int)(response.WarrantyUntil.Value - DateTimeOffset.UtcNow).TotalDays;
            if (response.WarrantyRemainingDays < 0)
                response.WarrantyRemainingDays = 0;
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
}
