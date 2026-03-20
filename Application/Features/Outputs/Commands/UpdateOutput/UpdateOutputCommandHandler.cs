using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;

using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        if(request.CurrentUserId is null)
        {
            return Error.BadRequest("CurrentUserId không được để trống.", "CurrentUserId");
        }

        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }

        if(output.BuyerId != request.CurrentUserId)
        {
            return Error.Unauthorized("Người dùng hiện tại không có quyền cập nhật đơn hàng này.", "CurrentUserId");
        }

        if(OrderLockStatus.DeliveryInfoLockedStatuses.Contains(output.StatusId ?? string.Empty))
        {
            if(string.Compare(request.CustomerName, output.CustomerName) != 0 ||
                string.Compare(request.CustomerPhone, output.CustomerPhone) != 0 ||
                string.Compare(request.CustomerAddress, output.CustomerAddress) != 0)
            {
                return Error.BadRequest(
                    "Trạng thái đơn hàng hiện tại không cho phép thay đổi thông tin giao hàng.",
                    "StatusId");
            }
        }

        if(OrderLockStatus.NotesLockedStatuses.Contains(output.StatusId ?? string.Empty))
        {
            if(string.Compare(request.Notes, output.Notes) != 0)
            {
                return Error.BadRequest("Trạng thái đơn hàng hiện tại không cho phép thay đổi ghi chú.", "StatusId");
            }
        }

        output.CustomerName = request.CustomerName;
        output.CustomerPhone = request.CustomerPhone;
        output.CustomerAddress = request.CustomerAddress;
        output.Notes = request.Notes;

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return updated.Adapt<OrderDetailResponse>();
    }
}
