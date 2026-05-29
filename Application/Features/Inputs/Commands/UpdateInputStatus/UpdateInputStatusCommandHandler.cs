using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInputStatus;

public sealed class UpdateInputStatusCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputStatusCommand, Result<InputDetailResponse>>
{
    public async Task<Result<InputDetailResponse>> Handle(
        UpdateInputStatusCommand request,
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
        if (InputStatus.IsCannotEdit(InventoryReceipt.StatusId))
        {
            return Error.BadRequest("Không thể sửa trạng thái phiếu nhập đã hoàn thành hoặc đã hủy.", "StatusId");
        }
        if (!InputStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
        }
        InventoryReceipt.StatusId = request.StatusId;
        if (string.Equals(request.StatusId, InputStatus.Finish, StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = currentUserContext.GetUserId();
            InventoryReceipt.InputDate = DateTimeOffset.UtcNow;
            InventoryReceipt.ConfirmedBy = currentUserId;
        }
        updateRepository.Update(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(InventoryReceipt.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<InputDetailResponse>();
    }
}
