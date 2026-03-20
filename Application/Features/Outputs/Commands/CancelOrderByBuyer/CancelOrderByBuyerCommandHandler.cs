using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CancelOrderByBuyer;

public sealed class CancelOrderByBuyerCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderByBuyerCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CancelOrderByBuyerCommand request,
        CancellationToken cancellationToken)
    {
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
            return Error.Forbidden("Bạn không có quyền hủy đơn hàng này.");
        }

        if(!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, OrderStatus.Cancelled))
        {
            return Error.BadRequest(
                $"Đơn hàng đang ở trạng thái '{output.StatusId}', không thể hủy trực tiếp. Vui lòng liên hệ quản trị viên để được hỗ trợ.",
                "StatusId");
        }

        output.StatusId = OrderStatus.Cancelled;
        output.LastStatusChangedAt = DateTimeOffset.UtcNow;

        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return updated.Adapt<OrderDetailResponse>();
    }
}
