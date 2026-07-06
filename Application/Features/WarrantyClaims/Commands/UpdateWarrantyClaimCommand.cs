using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public record UpdateWarrantyClaimCommand(
    int Id,
    int VehicleId,
    string IssueDescription,
    string? MediaUrls,
    string? ServiceCenterName,
    string? ManufacturerClaimNumber,
    int Status,
    string? ManufacturerDecision,
    bool IsRecall,
    decimal TotalPartsCost,
    decimal TotalLaborCost
) : IRequest<Result>;
