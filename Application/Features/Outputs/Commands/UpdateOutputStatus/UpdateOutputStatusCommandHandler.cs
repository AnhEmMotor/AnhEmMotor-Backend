using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Enums;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputStatusCommand, (OutputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, ErrorResponse? Error)> Handle(
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
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Id", Message = $"Không tìm thấy đơn hàng có ID {request.Id}." } ]
            });
        }

        if (!OrderStatus.IsValid(request.StatusId))
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "StatusId", Message = $"Trạng thái '{request.StatusId}' không hợp lệ." } ]
            });
        }

        if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "StatusId", Message = $"Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}" } ]
            });
        }

        if (request.StatusId == OrderStatus.Completed)
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
                        return (null, new ErrorResponse
                        {
                            Errors = [ new ErrorDetail { Field = "Products", Message = $"Sản phẩm ID {outputInfo.ProductId} không đủ tồn kho. Hiện có: {stock}, cần: {outputInfo.Count.Value}" } ]
                        });
                    }
                }
            }

            await updateRepository.ProcessCOGSForCompletedOrderAsync(
                output.Id,
                cancellationToken)
                .ConfigureAwait(false);
        }

        output.StatusId = request.StatusId;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(
            output.Id,
            cancellationToken)
            .ConfigureAwait(false);

        return (updated!.Adapt<OutputResponse>(), null);
    }
}
