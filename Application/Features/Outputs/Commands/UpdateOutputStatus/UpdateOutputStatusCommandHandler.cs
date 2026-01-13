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
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputStatusCommand, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(
        UpdateOutputStatusCommand request,
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

        if(!OrderStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
        }

        if(!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            return Error.BadRequest($"Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}", "StatusId");
        }

        if(string.Compare(request.StatusId, OrderStatus.Completed) == 0)
        {
            output.CreatedBy = request.CurrentUserId;
        } else
        {
            foreach(var outputInfo in output.OutputInfos)
            {
                if(outputInfo.ProductVarientId.HasValue && outputInfo.Count.HasValue)
                {
                    var stock = await readRepository.GetStockQuantityByVariantIdAsync(
                        outputInfo.ProductVarientId.Value,
                        cancellationToken)
                        .ConfigureAwait(false);

                    if(stock < outputInfo.Count.Value)
                    {
                        return Error.BadRequest($"Sản phẩm ID {outputInfo.ProductVarientId} không đủ tồn kho. Hiện có: {stock}, cần: {outputInfo.Count.Value}", "Products");
                    }
                }
            }

            await updateRepository.ProcessCOGSForCompletedOrderAsync(output.Id, cancellationToken).ConfigureAwait(false);
        }

        output.StatusId = request.StatusId;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return updated.Adapt<OutputResponse>();
    }
}
