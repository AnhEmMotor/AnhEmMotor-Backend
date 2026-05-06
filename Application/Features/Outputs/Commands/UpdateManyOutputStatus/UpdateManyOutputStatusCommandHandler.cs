using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services.HR;
using Domain.Constants.Order;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed class UpdateManyOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    ICommissionService commissionService,
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
        bool isCompleting = string.Compare(request.StatusId, OrderStatus.Completed) == 0;
        if (isCompleting && outputsList.Count > 0)
        {
            var productDemands = new Dictionary<int, int>();
            foreach (var output in outputsList)
            {
                if (output.OutputInfos == null)
                    continue;
                foreach (var info in output.OutputInfos)
                {
                    if (info.ProductVarientId.HasValue && info.Count.HasValue)
                    {
                        if (productDemands.ContainsKey(info.ProductVarientId.Value))
                        {
                            productDemands[info.ProductVarientId.Value] += info.Count.Value;
                        } else
                        {
                            productDemands[info.ProductVarientId.Value] = info.Count.Value;
                        }
                    }
                }
            }
            foreach (var kvp in productDemands)
            {
                var variantId = kvp.Key;
                var totalNeeded = kvp.Value;
                var currentStock = await readRepository.GetStockQuantityByVariantIdAsync(variantId, cancellationToken)
                    .ConfigureAwait(false);
                if (currentStock < totalNeeded)
                {
                    errors.Add(
                        Error.BadRequest(
                            $"Sản phẩm ID {variantId} không đủ tồn kho. Tổng kho hiện có: {currentStock}, Tổng đơn hàng cần: {totalNeeded}, Thiếu: {totalNeeded - currentStock}",
                            "Products"));
                }
            }
        }
        if (errors.Count > 0)
        {
            return errors;
        }
        if (isCompleting)
        {
            foreach (var output in outputsList)
            {
                await updateRepository.ProcessCOGSForCompletedOrderAsync(output.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        foreach (var output in outputsList)
        {
            output.StatusId = request.StatusId;
            updateRepository.Update(output);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (isCompleting)
        {
            foreach (var output in outputsList)
            {
                await commissionService.CalculateAndRecordCommissionAsync(output.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        } else if (request.StatusId == OrderStatus.Cancelled || request.StatusId == OrderStatus.Refunded)
        {
            foreach (var output in outputsList)
            {
                await commissionService.VoidCommissionAsync(output.Id, cancellationToken).ConfigureAwait(false);
            }
        }
        return outputsList.Adapt<List<OutputItemResponse>>();
    }
}
