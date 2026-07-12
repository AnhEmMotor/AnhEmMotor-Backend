using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public record CreateWarrantyClaimCommand(
    int VehicleId,
    string IssueDescription,
    string? MediaUrls,
    string? ServiceCenterName,
    bool IsRecall,
    decimal TotalPartsCost,
    decimal TotalLaborCost,
    List<WarrantyClaimPartRequest>? Parts
) : IRequest<Result<int>>;

public class WarrantyClaimPartRequest
{
    public string PartName { get; set; } = string.Empty;

    public string PartCode { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Status { get; set; }
}
