using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using System;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleHistory;

public class GetCustomerVehicleHistoryQueryHandler(IVehicleReadRepository vehicleRepository) : IRequestHandler<GetCustomerVehicleHistoryQuery, Result<CustomerVehicleHistoryResponse>>
{
    public async Task<Result<CustomerVehicleHistoryResponse>> Handle(
        GetCustomerVehicleHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle == null || vehicle.UserId != request.UserId)
        {
            return Result<CustomerVehicleHistoryResponse>.Failure(
                Error.NotFound("Không tìm thấy thông tin xe.", "VehicleId"));
        }
        var response = new CustomerVehicleHistoryResponse();
        response.PurchaseHistory
            .Add(
                new PurchaseHistoryDto
                {
                    Id = 1,
                    PurchaseDate = vehicle.PurchaseDate.DateTime,
                    InvoiceNumber = "HD-" + vehicle.VinNumber,
                    SellerName = "AnhEmMotor Showroom",
                    Amount = vehicle.ImportPrice > 0 ? vehicle.ImportPrice : 89500000m,
                    Notes = "Mua mới"
                });
        response.WarrantyHistory
            .Add(
                new WarrantyHistoryDto
                {
                    Id = 1,
                    StartDate = vehicle.PurchaseDate.DateTime.AddMonths(1),
                    ProviderName = "Honda VN",
                    PolicyNumber = "POL-" + vehicle.VinNumber,
                    Description = "Bảo dưỡng định kỳ",
                    CoverageAmount = 500000m,
                    Status = "completed"
                });
        return Result<CustomerVehicleHistoryResponse>.Success(response);
    }
}
