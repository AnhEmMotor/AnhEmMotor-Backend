using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Common.Models;
using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed class UpdateManyOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyOutputStatusCommand, (List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateManyOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Common.Models.ErrorDetail>();

        if(!OrderStatus.IsValid(request.StatusId))
        {
            errors.Add(
                new Common.Models.ErrorDetail
                {
                    Field = "StatusId",
                    Message = $"Trạng thái '{request.StatusId}' không hợp lệ."
                });
        }

        var outputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        var outputsList = outputs.ToList();

        var foundIds = outputsList.Select(o => o.Id).ToList();
        var missingIds = request.Ids.Except(foundIds).ToList();

        if(missingIds.Count != 0)
        {
            errors.Add(
                new Common.Models.ErrorDetail
                {
                    Field = "Ids",
                    Message = $"Không tìm thấy {missingIds.Count} đơn hàng: {string.Join(", ", missingIds)}"
                });
        }

        foreach(var output in outputsList)
        {
            if(!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
            {
                var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
                errors.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message =
                            $"Đơn hàng ID {output.Id}: Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}"
                    });
            }
        }

        if(string.Compare(request.StatusId, OrderStatus.Completed) == 0 && outputsList.Count > 0)
        {
            var productDemands = new Dictionary<int, int>();

            foreach(var output in outputsList)
            {
                if(output.OutputInfos == null)
                    continue;

                foreach(var info in output.OutputInfos)
                {
                    if(info.ProductVarientId.HasValue && info.Count.HasValue)
                    {
                        if(productDemands.ContainsKey(info.ProductVarientId.Value))
                        {
                            productDemands[info.ProductVarientId.Value] += info.Count.Value;
                        } else
                        {
                            productDemands[info.ProductVarientId.Value] = info.Count.Value;
                        }
                    }
                }
            }

            foreach(var kvp in productDemands)
            {
                var variantId = kvp.Key;
                var totalNeeded = kvp.Value;

                var currentStock = await readRepository.GetStockQuantityByVariantIdAsync(variantId, cancellationToken)
                    .ConfigureAwait(false);

                if(currentStock < totalNeeded)
                {
                    errors.Add(
                        new Common.Models.ErrorDetail
                        {
                            Field = "Products",
                            Message =
                                $"Sản phẩm ID {variantId} không đủ tồn kho. Tổng kho hiện có: {currentStock}, Tổng đơn hàng cần: {totalNeeded}, Thiếu: {totalNeeded - currentStock}"
                        });
                }
            }
        }

        if(errors.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errors });
        }

        if(string.Compare(request.StatusId, OrderStatus.Completed) == 0)
        {
            foreach(var output in outputsList)
            {
                await updateRepository.ProcessCOGSForCompletedOrderAsync(output.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        foreach(var output in outputsList)
        {
            output.StatusId = request.StatusId;
            updateRepository.Update(output);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (outputsList.Adapt<List<OutputResponse>>(), null);
    }
}