using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListPurchaseRequestsForChat;

public class ListPurchaseRequestsForChatQueryHandler(
    IPurchaseRequestReadRepository purchaseRequestReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListPurchaseRequestsForChatQuery, Result<ChatToolEnvelope<ChatPurchaseRequestListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatPurchaseRequestListItemDto>>> Handle(
        ListPurchaseRequestsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var statusId = string.IsNullOrWhiteSpace(request.StatusId) ? null : request.StatusId.Trim();
        var sieveModel = new SieveModel
        {
            Filters = statusId is null ? null : $"Status=={statusId}",
            Sorts = "-CreatedAt",
            Page = 1,
            PageSize = limit
        };
        var result = await purchaseRequestReadRepository
            .GetPagedAsync<PurchaseRequestListResponse>(sieveModel, DataFetchMode.ActiveOnly, cancellationToken)
            .ConfigureAwait(false);
        var items = result.Items ?? [];
        var dtos = items
            .Select(
                pr => new ChatPurchaseRequestListItemDto
                {
                    PurchaseRequestId = pr.Id,
                    Status = pr.Status,
                    Note = pr.Note,
                    CreatedByName = pr.CreatedByName,
                    TotalItems = pr.TotalItems,
                    CreatedAt = pr.CreatedAt
                })
            .ToList();
        var totalCount = (int)(result.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatPurchaseRequestListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (statusId is not null)
        {
            filtersApplied["Trạng thái"] = statusId;
        }
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IPurchaseRequestReadRepository.GetPagedAsync",
            filtersApplied,
            "yeu-cau-mua-hang",
            null);
        return ChatToolEnvelope<ChatPurchaseRequestListItemDto>.Wrap(inner, meta);
    }
}
