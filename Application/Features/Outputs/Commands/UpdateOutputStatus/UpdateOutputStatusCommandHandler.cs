using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOutputStatusCommand, (OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
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
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Không tìm thấy đơn hàng có ID {request.Id}."
                    } ]
            });
        }

        if(!OrderStatus.IsValid(request.StatusId))
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message = $"Trạng thái '{request.StatusId}' không hợp lệ."
                    } ]
            });
        }

        if(!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message =
                            $"Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}"
                    } ]
            });
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
                        return (null, new Common.Models.ErrorResponse
                        {
                            Errors =
                                [ new Common.Models.ErrorDetail
                                {
                                    Field = "Products",
                                    Message =
                                        $"Sản phẩm ID {outputInfo.ProductVarientId} không đủ tồn kho. Hiện có: {stock}, cần: {outputInfo.Count.Value}"
                                } ]
                        });
                    }
                }
            }

            await updateRepository.ProcessCOGSForCompletedOrderAsync(output.Id, cancellationToken).ConfigureAwait(false);
        }

        output.StatusId = request.StatusId;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);

        return (updated!.Adapt<OutputResponse>(), null);
    }
}
