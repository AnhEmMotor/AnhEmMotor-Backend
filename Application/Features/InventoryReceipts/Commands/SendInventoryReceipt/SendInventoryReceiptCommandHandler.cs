using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Services;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.SendInventoryReceipt;

public class SendInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<SendInventoryReceiptCommand, Result<InventoryReceiptDetailResponse>>
{
    public async Task<Result<InventoryReceiptDetailResponse>> Handle(
        SendInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var inventoryReceipt = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (inventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }
        if (!string.Equals(inventoryReceipt.StatusId, InventoryReceiptStatus.Draft, StringComparison.OrdinalIgnoreCase))
        {
            return Error.BadRequest("Chỉ có thể gửi phiếu nhập ở trạng thái nháp (draft).", "StatusId");
        }
        inventoryReceipt.StatusId = InventoryReceiptStatus.Sent;
        inventoryReceipt.SentBy = currentUserContext.GetUserId();
        updateRepository.Update(inventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(inventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<InventoryReceiptDetailResponse>();
    }
}
