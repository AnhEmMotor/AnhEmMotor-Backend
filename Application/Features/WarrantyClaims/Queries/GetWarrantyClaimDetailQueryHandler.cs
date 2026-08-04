using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Constants;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimDetailQueryHandler(IWarrantyClaimReadRepository repo) : IRequestHandler<GetWarrantyClaimDetailQuery, Result<WarrantyClaimDetailResponse?>>
{
    public async Task<Result<WarrantyClaimDetailResponse?>> Handle(GetWarrantyClaimDetailQuery req, CancellationToken ct)
    {
        var claim = await repo.GetDetailByIdAsync(req.Id, ct, DataFetchMode.All).ConfigureAwait(false);
        if (claim is null)
            return Result<WarrantyClaimDetailResponse?>.Failure(
                [Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={req.Id}", "Id")]);
        var vehicle = claim.Vehicle;
        var lead = vehicle?.Lead;
        string? vehicleInfo = null;
        if (vehicle != null)
        {
            var parts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(vehicle.VinNumber))
                parts.Add(vehicle.VinNumber);
            if (!string.IsNullOrWhiteSpace(vehicle.EngineNumber))
                parts.Add(vehicle.EngineNumber);
            vehicleInfo = parts.Count > 0 ? string.Join(" | ", parts) : null;
        }
        var variantColor = vehicle?.ProductVariantColor;
        string? vehicleColor = variantColor?.ColorName ?? variantColor?.ColorCode;
        string? vehicleYear = vehicle?.PurchaseDate.Year.ToString();
        var user = vehicle?.User;
        var response = new WarrantyClaimDetailResponse
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            VehicleId = claim.VehicleId,
            VehicleInfo = vehicleInfo,
            VehiclePlate = vehicle?.LicensePlate,
            CustomerName = lead != null ? lead.FullName : user?.FullName,
            CustomerPhone = lead != null ? lead.PhoneNumber : user?.PhoneNumber,
            CustomerAddress = lead?.Address,
            StatusText = WarrantyClaimStatus.GetLabel(claim.Status),
            VehicleVin = vehicle?.VinNumber,
            IssueDescription = claim.IssueDescription,
            MediaUrls = string.IsNullOrWhiteSpace(claim.MediaUrls)
                ? new List<string>()
                : claim.MediaUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            ServiceCenterName = claim.ServiceCenterName,
            ManufacturerClaimNumber = claim.ManufacturerClaimNumber,
            Status = claim.Status,
            ManufacturerDecision = claim.ManufacturerDecision,
            IsRecall = claim.IsRecall,
            TotalPartsCost = claim.TotalPartsCost,
            TotalLaborCost = claim.TotalLaborCost,
            Parts =
                claim.WarrantyClaimParts
                    .Where(p => p.DeletedAt == null)
                    .Select(
                        p => new WarrantyClaimPartResponse
                    {
                        Id = p.Id,
                        WarrantyClaimId = p.WarrantyClaimId,
                        PartName = p.PartName,
                        PartCode = p.PartCode,
                        UnitPrice = p.UnitPrice,
                        Status = p.Status
                    })
                    .ToList(),
            VehicleColor = vehicleColor,
            VehicleYear = vehicleYear,
            WarrantyRemaining = null,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt
        };
        return Result<WarrantyClaimDetailResponse?>.Success(response);
    }
}
