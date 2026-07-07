using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public record UpdateWarrantyClaimCommand(
    int Id,
    int? VehicleId = null,
    string? IssueDescription = null,
    string? MediaUrls = null,
    string? ServiceCenterName = null,
    string? ManufacturerClaimNumber = null,
    int? Status = null,
    string? ManufacturerDecision = null,
    bool? IsRecall = null,
    decimal? TotalPartsCost = null,
    decimal? TotalLaborCost = null
) : IRequest<Result>;
