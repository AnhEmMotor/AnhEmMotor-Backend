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
    IServerDateProvider dateProvider)
    : IRequestHandler<GetPurchaseRequestDetailForChatQuery, Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>> Handle(
        GetPurchaseRequestDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await purchaseRequestReadRepository
            .GetByIdWithDetailsAsync(request.PurchaseRequestId, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>.Failure(
                Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.PurchaseRequestId}.", "Id"));
        }

        var response = entity.Adapt<PurchaseRequestDetailResponse>();
        var dto = new ChatPurchaseRequestDetailDto
        {
            PurchaseRequestId = response.Id,
            Status = response.Status,
            Note = response.Note,
            CreatedByName = response.CreatedByName,
            CreatedAt = response.CreatedAt,
            Items = response.Items
                .Select(
                    i => new ChatPurchaseRequestDetailItemDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        SupplierName = i.SupplierName,
                        UnitPrice = i.UnitPrice
                    })
                .ToList()
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IPurchaseRequestReadRepository.GetByIdWithDetailsAsync",
            new Dictionary<string, string>(),
            "yeu-cau-mua-hang",
            "VND");

        return ChatToolEnvelope<ChatPurchaseRequestDetailDto>.WrapSingle(dto, meta);
    }
}
