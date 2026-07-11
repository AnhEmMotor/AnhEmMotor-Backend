using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehicleHistory;

public class GetVehicleHistoryQueryHandler(
    IVehicleReadRepository vehicleReadRepository,
    IVehiclePurchaseHistoryReadRepository purchaseHistoryReadRepository,
    IVehicleWarrantyHistoryReadRepository warrantyHistoryReadRepository) : IRequestHandler<GetVehicleHistoryQuery, Result<VehicleHistoryResponse>>
{
    public async Task<Result<VehicleHistoryResponse>> Handle(GetVehicleHistoryQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<VehicleHistoryResponse>.Failure([Error.NotFound("Vehicle not found.", "VehicleId")]);
        }

        var purchaseHistory = await purchaseHistoryReadRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        var warrantyHistory = await warrantyHistoryReadRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);

        return Result<VehicleHistoryResponse>.Success(new VehicleHistoryResponse
        {
            PurchaseHistory = purchaseHistory.Select(p => new VehiclePurchaseHistoryItem
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                UserId = p.UserId,
                PurchaseDate = p.PurchaseDate,
                InvoiceNumber = p.InvoiceNumber,
                Amount = p.Amount,
                SellerName = p.SellerName,
                Notes = p.Notes
            }).ToList(),
            WarrantyHistory = warrantyHistory.Select(w => new VehicleWarrantyHistoryItem
            {
                Id = w.Id,
                VehicleId = w.VehicleId,
                UserId = w.UserId,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                ProviderName = w.ProviderName,
                PolicyNumber = w.PolicyNumber,
                Description = w.Description,
                Status = w.Status,
                CoverageAmount = w.CoverageAmount
            }).ToList()
        });
    }
}
