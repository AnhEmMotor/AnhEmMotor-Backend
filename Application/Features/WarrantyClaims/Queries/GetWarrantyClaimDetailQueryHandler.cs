using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimDetailQueryHandler(
 IWarrantyClaimReadRepository repo) : IRequestHandler<GetWarrantyClaimDetailQuery, Result<WarrantyClaimResponse?>>
{
 public async Task<Result<WarrantyClaimResponse?>> Handle(GetWarrantyClaimDetailQuery req, CancellationToken ct)
 {
  var claim = await repo.GetDetailByIdAsync(req.Id, ct, DataFetchMode.All).ConfigureAwait(false);
  if (claim is null)
   return Result<WarrantyClaimResponse?>.Failure([Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={req.Id}", "Id")]);

  var vehicle = claim.Vehicle;
  var lead = vehicle?.Lead;
  string? vehicleInfo = null;
  if (vehicle != null)
  {
   var parts = new List<string?>();
   if (!string.IsNullOrWhiteSpace(vehicle.VinNumber)) parts.Add(vehicle.VinNumber);
   if (!string.IsNullOrWhiteSpace(vehicle.EngineNumber)) parts.Add(vehicle.EngineNumber);
   vehicleInfo = parts.Count > 0 ? string.Join(" | ", parts) : null;
  }

  var variantColor = vehicle?.ProductVariantColor;
  string? vehicleColor = variantColor?.ColorName ?? variantColor?.ColorCode;
  string? vehicleYear = vehicle?.PurchaseDate.Year.ToString();

  var response = new WarrantyClaimResponse
  {
   Id = claim.Id,
   ClaimNumber = claim.ClaimNumber,
   VehicleId = claim.VehicleId,
   VehicleInfo = vehicleInfo,
   VehiclePlate = vehicle?.LicensePlate,
   CustomerName = lead?.FullName,
   CustomerPhone = lead?.PhoneNumber,
   CustomerAddress = lead?.Address,
   StatusText = WarrantyClaimStatus.GetLabel(claim.Status),
   VehicleVin = vehicle?.VinNumber,
   IssueDescription = claim.IssueDescription,
   MediaUrls = claim.MediaUrls,
   ServiceCenterName = claim.ServiceCenterName,
   ManufacturerClaimNumber = claim.ManufacturerClaimNumber,
   Status = claim.Status,
   ManufacturerDecision = claim.ManufacturerDecision,
   IsRecall = claim.IsRecall,
   TotalPartsCost = claim.TotalPartsCost,
   TotalLaborCost = claim.TotalLaborCost,
   Parts = claim.WarrantyClaimParts
   .Where(p => p.DeletedAt == null)
   .Select(p => new WarrantyClaimPartResponse
   {
    Id = p.Id,
    WarrantyClaimId = p.WarrantyClaimId,
    PartName = p.PartName,
    PartCode = p.PartCode,
    UnitPrice = p.UnitPrice,
    Status = p.Status
   }).ToList(),
   CreatedAt = claim.CreatedAt,
   UpdatedAt = claim.UpdatedAt,
   IsDeleted = claim.DeletedAt != null
  };

  return Result<WarrantyClaimResponse?>.Success(response);
 }
}
