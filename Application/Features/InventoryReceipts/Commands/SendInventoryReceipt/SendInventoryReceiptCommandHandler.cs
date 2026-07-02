using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.SendInventoryReceipt;

public class SendInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    IInventoryReceiptInsertRepository insertRepository,
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
        var oldStatusId = inventoryReceipt.StatusId;
        var currentUserId = currentUserContext.GetUserId();
        
        inventoryReceipt.StatusId = InventoryReceiptStatus.Sent;
        inventoryReceipt.SentBy = currentUserId;
        updateRepository.Update(inventoryReceipt);

        var receiptAuditLogs = new List<Domain.Entities.InventoryReceiptAuditLog>
        {
            new Domain.Entities.InventoryReceiptAuditLog
            {
                InventoryReceipt = inventoryReceipt,
                Action = "Update",
                ChangedById = currentUserId,
                ChangedAt = DateTimeOffset.UtcNow,
                OldStatusId = oldStatusId,
                NewStatusId = inventoryReceipt.StatusId,
                OldNotes = inventoryReceipt.Notes,
                NewNotes = inventoryReceipt.Notes
            }
        };
        await insertRepository.InsertAuditLogsAsync(receiptAuditLogs, cancellationToken).ConfigureAwait(false);
        
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(inventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<InventoryReceiptDetailResponse>();
    }
}
