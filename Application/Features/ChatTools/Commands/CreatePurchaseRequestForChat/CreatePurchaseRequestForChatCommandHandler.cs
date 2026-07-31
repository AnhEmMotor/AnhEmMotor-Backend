using Application.ApiContracts.PurchaseRequest.Requests;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using MediatR;

namespace Application.Features.ChatTools.Commands.CreatePurchaseRequestForChat;

/// <summary>Không viết lại logic tạo PR — tái sử dụng <see cref="CreatePurchaseRequestCommand"/> qua ISender
/// (cùng validation, audit log, unit of work như API thường).</summary>
public class CreatePurchaseRequestForChatCommandHandler(ISender sender, IServerDateProvider dateProvider)
    : IRequestHandler<CreatePurchaseRequestForChatCommand, Result<ChatToolEnvelope<ChatCreatePurchaseRequestResultDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatCreatePurchaseRequestResultDto>>> Handle(
        CreatePurchaseRequestForChatCommand request,
        CancellationToken cancellationToken)
    {
        var innerCommand = new CreatePurchaseRequestCommand
        {
            Note = request.Note,
            Items =
                [.. request.Items
                    .Select(
                        item => new CreatePurchaseRequestItemRequest
                        {
                            ProductVariantId = item.ProductVariantId,
                            Quantity = item.Quantity
                        })]
        };

        var result = await sender.Send(innerCommand, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || result.Value is null)
        {
            return result.Errors;
        }

        var dto = new ChatCreatePurchaseRequestResultDto
        {
            PurchaseRequestId = result.Value.Id,
            Status = result.Value.Status,
            ItemCount = result.Value.Items.Count
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "CreatePurchaseRequestCommand",
            new Dictionary<string, string>(),
            "yeu-cau-mua-hang",
            null);

        return ChatToolEnvelope<ChatCreatePurchaseRequestResultDto>.WrapSingle(dto, meta);
    }
}
