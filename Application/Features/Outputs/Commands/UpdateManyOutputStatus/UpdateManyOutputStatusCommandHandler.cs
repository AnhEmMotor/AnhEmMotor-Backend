using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed class UpdateManyOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyOutputStatusCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(
        UpdateManyOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!OrderStatus.IsValid(request.NewStatusId))
        {
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "NewStatusId", Message = $"Trạng thái '{request.NewStatusId}' không hợp lệ." } ]
            };
        }

        var outputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken)
            .ConfigureAwait(false);

        var outputsList = outputs.ToList();

        if (outputsList.Count != request.Ids.Count)
        {
            var foundIds = outputsList.Select(o => o.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Ids", Message = $"Không tìm thấy {missingIds.Count} đơn hàng: {string.Join(", ", missingIds)}" } ]
            };
        }

        foreach (var output in outputsList)
        {
            if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.NewStatusId))
            {
                var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
                return new ErrorResponse
                {
                    Errors = [ new ErrorDetail { Field = "NewStatusId", Message = $"Đơn hàng ID {output.Id}: Không thể chuyển từ '{output.StatusId}' sang '{request.NewStatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}" } ]
                };
            }
        }

        if (request.NewStatusId == OrderStatus.Completed)
        {
            foreach (var output in outputsList)
            {
                await updateRepository.ProcessCOGSForCompletedOrderAsync(
                    output.Id,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        foreach (var output in outputsList)
        {
            output.StatusId = request.NewStatusId;
            updateRepository.Update(output);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
