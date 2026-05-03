using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;

using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed class UpdateManyOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyOutputStatusCommand, Result<List<OutputItemResponse>?>>
{
    public async Task<Result<List<OutputItemResponse>?>> Handle(
        UpdateManyOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
        var outputs = await readRepository.GetByIdAsync(request.Ids!, cancellationToken).ConfigureAwait(false);
        var outputsList = outputs.ToList();
        var foundIds = outputsList.Select(o => o.Id).ToList();
        var missingIds = request.Ids!.Except(foundIds).ToList();
        if (missingIds.Count != 0)
        {
            errors.Add(
                Error.NotFound($"Không tìm thấy {missingIds.Count} đơn hàng: {string.Join(", ", missingIds)}", "Ids"));
        }
        foreach (var output in outputsList)
        {
            if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
            {
                var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
                errors.Add(
                    Error.BadRequest(
                        $"Đơn hàng ID {output.Id}: Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}",
                        "StatusId"));
            }
        }
        if (errors.Count > 0)
        {
            return errors;
        }

        // Thực hiện cập nhật trạng thái và kiểm tra tồn kho trong 1 transaction duy nhất
        // Để đảm bảo tính Atomic (Tất cả thành công hoặc không gì thay đổi)
        foreach (var output in outputsList)
        {
            if (string.Compare(request.StatusId, OrderStatus.Completed) == 0)
            {
                var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, true, cancellationToken)
                    .ConfigureAwait(false);
                if (result.IsFailure) return result.Errors!;
            }
            else if (string.Compare(request.StatusId, OrderStatus.Delivering) == 0)
            {
                var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, false, cancellationToken)
                    .ConfigureAwait(false);
                if (result.IsFailure) return result.Errors!;
            }

            output.StatusId = request.StatusId;
            updateRepository.Update(output);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return outputsList.Adapt<List<OutputItemResponse>>();
    }
}
