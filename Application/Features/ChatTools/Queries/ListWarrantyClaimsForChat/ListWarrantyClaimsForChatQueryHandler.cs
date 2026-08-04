using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListWarrantyClaimsForChat;

public class ListWarrantyClaimsForChatQueryHandler(
    IWarrantyClaimReadRepository warrantyClaimReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListWarrantyClaimsForChatQuery, Result<ChatToolEnvelope<ChatWarrantyClaimListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatWarrantyClaimListItemDto>>> Handle(
        ListWarrantyClaimsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var claims = (await warrantyClaimReadRepository
            .GetAllWithDetailsAsync(DataFetchMode.ActiveOnly, cancellationToken)
            .ConfigureAwait(false)).AsEnumerable();
        var filtersApplied = new Dictionary<string, string>();
        if (int.TryParse(request.StatusId, out var statusId))
        {
            claims = claims.Where(c => c.Status == statusId);
            filtersApplied["StatusId"] = statusId.ToString();
        }
        var filtered = claims.OrderByDescending(c => c.CreatedAt).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = filtered
            .Take(limit)
            .Select(
                c => new ChatWarrantyClaimListItemDto
                {
                    ClaimId = c.Id,
                    VehicleInfo = c.Vehicle?.VinNumber ?? c.Vehicle?.LicensePlate,
                    CustomerName = c.Vehicle?.Lead?.FullName,
                    StatusId = c.Status,
                    StatusLabel = WarrantyClaimStatus.GetLabel(c.Status),
                    CreatedAt = c.CreatedAt
                })
            .ToList();
        var inner = new ChatToolResult<ChatWarrantyClaimListItemDto>(dtos, filtered.Count, filtered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IWarrantyClaimReadRepository.GetAllWithDetailsAsync",
            filtersApplied,
            "bao-hanh",
            null);
        return ChatToolEnvelope<ChatWarrantyClaimListItemDto>.Wrap(inner, meta);
    }
}
