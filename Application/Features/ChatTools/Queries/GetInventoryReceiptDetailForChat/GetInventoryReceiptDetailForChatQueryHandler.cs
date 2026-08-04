using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.InventoryReceipt;
using Mapster;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.GetInventoryReceiptDetailForChat;

public class GetInventoryReceiptDetailForChatQueryHandler(
    IInventoryReceiptReadRepository repository,
    IServerDateProvider dateProvider) : IRequestHandler<GetInventoryReceiptDetailForChatQuery, Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>>
{
    private const int MaxMatches = 5;

    public async Task<Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>> Handle(
        GetInventoryReceiptDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword.Trim();
        var sieveModel = new SieveModel { Page = 1, PageSize = MaxMatches, Sorts = "-CreatedAt" };
        var paged = await repository.GetPagedAsync<InventoryReceiptListResponse>(
            sieveModel,
            filter: r => r.InventoryReceiptInfos
                .Any(
                    ii => ii.DeletedAt == null &&
                            ii.PurchaseRequestItem != null &&
                            ii.PurchaseRequestItem.Supplier != null &&
                            ii.PurchaseRequestItem.Supplier.Name != null &&
                            ii.PurchaseRequestItem.Supplier.Name.Contains(keyword)),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var matchIds = (paged.Items ?? []).Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var dtos = new List<ChatInventoryReceiptDetailDto>();
        foreach (var id in matchIds)
        {
            var receipt = await repository.GetByIdWithDetailsAsync(id, cancellationToken).ConfigureAwait(false);
            if (receipt is null)
            {
                continue;
            }
            var detail = receipt.Adapt<InventoryReceiptDetailResponse>();
            var items = (detail.Products ?? [])
                .Select(
                    p => new ChatInventoryReceiptDetailItemDto
                    {
                        Name = p.Name,
                        ColorName = p.ProductVariantColorName,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    })
                .ToList();
            var totalAmount = items.Sum(i => (i.Quantity ?? 0) * (i.UnitPrice ?? 0));
            dtos.Add(
                new ChatInventoryReceiptDetailDto
                {
                    Id = detail.Id,
                    CreatedAt = detail.CreatedAt,
                    StatusId = detail.StatusId,
                    SupplierName = detail.SupplierName,
                    CreatedByName = detail.CreatedByName,
                    Notes = detail.Notes,
                    TotalAmount = totalAmount,
                    Items = items
                });
        }
        var inner = new ChatToolResult<ChatInventoryReceiptDetailDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IInventoryReceiptReadRepository.GetPagedAsync+GetByIdWithDetailsAsync",
            new Dictionary<string, string> { ["Keyword"] = keyword },
            "phieu-nhap-kho-chi-tiet",
            "VND");
        return ChatToolEnvelope<ChatInventoryReceiptDetailDto>.Wrap(inner, meta);
    }
}
