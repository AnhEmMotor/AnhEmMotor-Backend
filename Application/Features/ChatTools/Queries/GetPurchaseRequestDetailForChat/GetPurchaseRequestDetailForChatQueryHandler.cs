using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.PurchaseRequest;
using Mapster;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPurchaseRequestDetailForChat;

public class GetPurchaseRequestDetailForChatQueryHandler(
    IPurchaseRequestReadRepository purchaseRequestReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetPurchaseRequestDetailForChatQuery, Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>>
{
    private const int MaxMatches = 5;

    public async Task<Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>> Handle(
        GetPurchaseRequestDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword.Trim();
        var dtos = new List<ChatPurchaseRequestDetailDto>();
        if (keyword.Length > 0)
        {
            var ids = await purchaseRequestReadRepository
                .SearchIdsBySupplierNameAsync(keyword, MaxMatches, cancellationToken)
                .ConfigureAwait(false);
            foreach (var id in ids)
            {
                var entity = await purchaseRequestReadRepository
                    .GetByIdWithDetailsAsync(id, cancellationToken)
                    .ConfigureAwait(false);
                if (entity is null)
                {
                    continue;
                }
                var response = entity.Adapt<PurchaseRequestDetailResponse>();
                dtos.Add(
                    new ChatPurchaseRequestDetailDto
                    {
                        PurchaseRequestId = response.Id,
                        Status = response.Status,
                        Note = response.Note,
                        CreatedByName = response.CreatedByName,
                        CreatedAt = response.CreatedAt,
                        Items =
                            response.Items
                                    .Select(
                                        i => new ChatPurchaseRequestDetailItemDto
                                    {
                                        ProductName = i.ProductName,
                                        Quantity = i.Quantity,
                                        SupplierName = i.SupplierName,
                                        UnitPrice = i.UnitPrice
                                    })
                                    .ToList()
                    });
            }
        }
        var inner = new ChatToolResult<ChatPurchaseRequestDetailDto>(dtos, dtos.Count, false);
        var filtersApplied = new Dictionary<string, string> { ["Nhà cung cấp"] = keyword };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IPurchaseRequestReadRepository.SearchIdsBySupplierNameAsync+GetByIdWithDetailsAsync",
            filtersApplied,
            "yeu-cau-mua-hang",
            "VND");
        return ChatToolEnvelope<ChatPurchaseRequestDetailDto>.Wrap(inner, meta);
    }
}
