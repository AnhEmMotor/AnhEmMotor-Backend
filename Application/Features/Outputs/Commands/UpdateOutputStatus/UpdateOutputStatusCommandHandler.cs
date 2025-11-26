using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputStatusCommand, OutputResponse>
{
    public async Task<OutputResponse> Handle(
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
            throw new InvalidOperationException($"Không tìm thấy đơn hàng có ID {request.Id}.");
        }

        if (!OrderStatus.IsValid(request.NewStatusId))
        {
            throw new InvalidOperationException($"Trạng thái '{request.NewStatusId}' không hợp lệ.");
        }

        if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.NewStatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            throw new InvalidOperationException(
                $"Không thể chuyển từ '{output.StatusId}' sang '{request.NewStatusId}'. " +
                $"Chỉ được chuyển sang: {string.Join(", ", allowed)}");
        }

        if (request.NewStatusId == OrderStatus.Completed)
        {
            foreach (var outputInfo in output.OutputInfos)
            {
                if (outputInfo.ProductId.HasValue && outputInfo.Count.HasValue)
                {
                    var stock = await readRepository.GetStockQuantityByVariantIdAsync(
                        outputInfo.ProductId.Value,
                        cancellationToken)
                        .ConfigureAwait(false);

                    if (stock < outputInfo.Count.Value)
                    {
                        throw new InvalidOperationException(
                            $"Sản phẩm ID {outputInfo.ProductId} không đủ tồn kho. " +
                            $"Hiện có: {stock}, cần: {outputInfo.Count.Value}");
                    }
                }
            }

            await updateRepository.ProcessCOGSForCompletedOrderAsync(
                output.Id,
                cancellationToken)
                .ConfigureAwait(false);
        }

        output.StatusId = request.NewStatusId;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(
            output.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return updated!.Adapt<OutputResponse>();
    }
}
