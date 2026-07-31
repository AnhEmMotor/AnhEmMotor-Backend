using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.InventoryReceipt;
using Mapster;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryReceiptDetailForChat;

public class GetInventoryReceiptDetailForChatQueryHandler(
    IInventoryReceiptReadRepository repository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetInventoryReceiptDetailForChatQuery, Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>> Handle(
        GetInventoryReceiptDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var receipt = await repository.GetByIdWithDetailsAsync(request.ReceiptId, cancellationToken).ConfigureAwait(false);
        if (receipt is null)
        {
            return Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>.Failure(
                Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.ReceiptId}."));
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
        var dto = new ChatInventoryReceiptDetailDto
        {
            Id = detail.Id,
            CreatedAt = detail.CreatedAt,
            StatusId = detail.StatusId,
            SupplierName = detail.SupplierName,
            CreatedByName = detail.CreatedByName,
            Notes = detail.Notes,
            TotalAmount = totalAmount,
            Items = items
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IInventoryReceiptReadRepository.GetByIdWithDetailsAsync",
            new Dictionary<string, string> { ["ReceiptId"] = request.ReceiptId.ToString() },
            "phieu-nhap-kho-chi-tiet",
            "VND");
        return ChatToolEnvelope<ChatInventoryReceiptDetailDto>.WrapSingle(dto, meta);
    }
}
