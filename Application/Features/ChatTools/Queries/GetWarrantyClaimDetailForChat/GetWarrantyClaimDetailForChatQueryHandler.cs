using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarrantyClaimDetailForChat;

public class GetWarrantyClaimDetailForChatQueryHandler(
    IWarrantyClaimReadRepository warrantyClaimReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetWarrantyClaimDetailForChatQuery, Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>>
{
    private const int MaxMatches = 5;

    public async Task<Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>> Handle(
        GetWarrantyClaimDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword.Trim();
        var claims = await warrantyClaimReadRepository
            .GetAllWithDetailsAsync(DataFetchMode.ActiveOnly, cancellationToken)
            .ConfigureAwait(false);

        var matches = claims
            .Where(
                c => (c.Vehicle?.Lead?.FullName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (c.Vehicle?.LicensePlate?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var dtos = matches
            .Take(MaxMatches)
            .Select(
                claim =>
                {
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

                    return new ChatWarrantyClaimDetailDto
                    {
                        ClaimId = claim.Id,
                        ClaimNumber = claim.ClaimNumber,
                        VehicleInfo = vehicleInfo,
                        VehiclePlate = vehicle?.LicensePlate,
                        CustomerName = lead?.FullName,
                        CustomerPhone = lead?.PhoneNumber,
                        CustomerAddress = lead?.Address,
                        IssueDescription = claim.IssueDescription,
                        MediaUrls = claim.MediaUrls,
                        ServiceCenterName = claim.ServiceCenterName,
                        ManufacturerClaimNumber = claim.ManufacturerClaimNumber,
                        StatusId = claim.Status,
                        StatusLabel = WarrantyClaimStatus.GetLabel(claim.Status),
                        ManufacturerDecision = claim.ManufacturerDecision,
                        IsRecall = claim.IsRecall,
                        TotalPartsCost = claim.TotalPartsCost,
                        TotalLaborCost = claim.TotalLaborCost,
                        Parts = claim.WarrantyClaimParts
                            .Where(p => p.DeletedAt == null)
                            .Select(
                                p => new ChatWarrantyClaimPartDto
                                {
                                    PartName = p.PartName,
                                    PartCode = p.PartCode,
                                    UnitPrice = p.UnitPrice,
                                    Status = p.Status
                                })
                            .ToList(),
                        CreatedAt = claim.CreatedAt,
                        UpdatedAt = claim.UpdatedAt
                    };
                })
            .ToList();

        var inner = new ChatToolResult<ChatWarrantyClaimDetailDto>(dtos, matches.Count, matches.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IWarrantyClaimReadRepository.GetAllWithDetailsAsync",
            new Dictionary<string, string> { ["Keyword"] = keyword },
            "yeu-cau-bao-hanh",
            null);
        return ChatToolEnvelope<ChatWarrantyClaimDetailDto>.Wrap(inner, meta);
    }
}
