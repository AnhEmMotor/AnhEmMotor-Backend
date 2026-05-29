using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Services;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;

public sealed class UpdateInventoryReceiptStatusCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInventoryReceiptStatusCommand, Result<InventoryReceiptDetailResponse>>
{
    public async Task<Result<InventoryReceiptDetailResponse>> Handle(
        UpdateInventoryReceiptStatusCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }
        if (!string.Equals(InventoryReceipt.StatusId, InventoryReceiptStatus.Sent, StringComparison.OrdinalIgnoreCase))
        {
            return Error.BadRequest("Chỉ có thể duyệt hoặc từ chối phiếu nhập đang ở trạng thái đã gửi (sent).", "StatusId");
        }
        if (!string.Equals(request.StatusId, InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.StatusId, InventoryReceiptStatus.Reject, StringComparison.OrdinalIgnoreCase))
        {
            return Error.BadRequest("Trạng thái mới phải là phê duyệt (approve) hoặc từ chối (reject).", "StatusId");
        }
        InventoryReceipt.StatusId = request.StatusId;
        if (string.Equals(request.StatusId, InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = currentUserContext.GetUserId();
            InventoryReceipt.InventoryReceiptDate = DateTimeOffset.UtcNow;
            InventoryReceipt.ConfirmedBy = currentUserId;
        }
        updateRepository.Update(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(InventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<InventoryReceiptDetailResponse>();
    }
}
