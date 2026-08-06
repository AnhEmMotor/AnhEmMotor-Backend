using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.InventoryReceipt;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListInventoryReceiptsForChat;

public class ListInventoryReceiptsForChatQueryHandler(
    IInventoryReceiptReadRepository repository,
    IServerDateProvider dateProvider) : IRequestHandler<ListInventoryReceiptsForChatQuery, Result<ChatToolEnvelope<ChatInventoryReceiptListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatInventoryReceiptListItemDto>>> Handle(
        ListInventoryReceiptsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var filters = new Dictionary<string, string>();
        var sieveModel = new SieveModel { Page = 1, PageSize = limit, Sorts = "-CreatedAt" };
        if (request.FromDate.HasValue || request.ToDate.HasValue)
        {
            var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
            sieveModel.Filters = $"CreatedAt>={start:yyyy-MM-dd},CreatedAt<={end:yyyy-MM-dd}";
            filters["Khoảng ngày"] = ChatToolDateRange.FormatVietnamRange(start, end);
        }
        var paged = await repository.GetPagedAsync<InventoryReceiptListResponse>(
            sieveModel,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                x => new ChatInventoryReceiptListItemDto
                {
                    Id = x.Id,
                    CreatedAt = x.CreatedAt,
                    StatusId = x.StatusId,
                    SupplierName = x.SupplierName,
                    CreatedByName = x.CreatedByName,
                    TotalPayable = x.TotalPayable
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatInventoryReceiptListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IInventoryReceiptReadRepository.GetPagedAsync",
            filters,
            "phieu-nhap-kho",
            "VND");
        return ChatToolEnvelope<ChatInventoryReceiptListItemDto>.Wrap(inner, meta);
    }
}
