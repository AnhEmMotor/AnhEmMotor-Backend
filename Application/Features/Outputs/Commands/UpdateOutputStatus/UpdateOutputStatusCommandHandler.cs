using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputStatusCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        UpdateOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }
        if (!OrderStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
        }
        if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            return Error.BadRequest(
                $"Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}",
                "StatusId");
        }
        switch (request.StatusId)
        {
            case OrderStatus.Completed:
                output.FinishedBy = request.CurrentUserId;
                var deductionResult = await updateRepository.HandleInventoryTransactionAsync(
                    output.Id,
                    true,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (deductionResult.IsFailure)
                {
                    return Result<OrderDetailResponse>.Failure(deductionResult.Errors!);
                }
                break;
            case OrderStatus.Delivering:
                var checkResult = await updateRepository.HandleInventoryTransactionAsync(
                    output.Id,
                    false,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (checkResult.IsFailure)
                {
                    return Result<OrderDetailResponse>.Failure(checkResult.Errors!);
                }
                break;
            case OrderStatus.Cancelled:
            case OrderStatus.Refunding:
            case OrderStatus.Refunded:
            default:
                break;
        }
        output.StatusId = request.StatusId;
        output.LastStatusChangedAt = DateTimeOffset.UtcNow;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<OrderDetailResponse>();
    }
}