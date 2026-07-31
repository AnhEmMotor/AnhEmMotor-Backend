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
    public async Task<Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>> Handle(
        GetWarrantyClaimDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var claim = await warrantyClaimReadRepository
            .GetDetailByIdAsync(request.ClaimId, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (claim == null)
        {
            return Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>.Failure(
                Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={request.ClaimId}", "ClaimId"));
        }

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

        var dto = new ChatWarrantyClaimDetailDto
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

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IWarrantyClaimReadRepository.GetDetailByIdAsync",
            new Dictionary<string, string>(),
            "yeu-cau-bao-hanh",
            null);
        return ChatToolEnvelope<ChatWarrantyClaimDetailDto>.WrapSingle(dto, meta);
    }
}
