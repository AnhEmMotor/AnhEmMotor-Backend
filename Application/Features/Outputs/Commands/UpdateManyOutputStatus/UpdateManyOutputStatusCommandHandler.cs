using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using Domain.Constants.Product;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public class UpdateManyOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    ICommissionUpdateRepository commissionUpdateRepository,
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
            if (OrderVehicleAssignmentStatus.RequiresVehicleAssignment(request.StatusId))
            {
                var containsVehicleManagedProduct = output.OutputInfos
                    .Any(
                        oi => oi.ProductVariant?.Product?.ProductCategory != null &&
                            string.Equals(
                                oi.ProductVariant.Product.ProductCategory.ManagementType,
                                ProductManagementType.VinNumber,
                                StringComparison.OrdinalIgnoreCase));
                if (containsVehicleManagedProduct)
                {
                    errors.Add(
                        Error.BadRequest(
                            $"Đơn hàng ID {output.Id} chứa sản phẩm quản lý theo số khung, không thể cập nhật trạng thái hàng loạt sang '{request.StatusId}'. Vui lòng cập nhật riêng lẻ và truyền SelectedVehicleIds.",
                            "StatusId"));
                }
            }
        }
        bool isCompleting = string.Compare(request.StatusId, OrderStatus.Completed) == 0;
        if (isCompleting && outputsList.Count > 0)
        {
            var productDemands = new Dictionary<(int VariantId, int? ColorId), int>();
            foreach (var output in outputsList)
            {
                if (output.OutputInfos == null)
                    continue;
                foreach (var info in output.OutputInfos)
                {
                    if (info.ProductVariantId.HasValue && info.Count.HasValue)
                    {
                        var key = (info.ProductVariantId.Value, info.ProductVariantColorId);
                        if (productDemands.ContainsKey(key))
                        {
                            productDemands[key] += info.Count.Value;
                        } else
                        {
                            productDemands[key] = info.Count.Value;
                        }
                    }
                }
            }
            foreach (var kvp in productDemands)
            {
                var variantId = kvp.Key.VariantId;
                var colorId = kvp.Key.ColorId;
                var totalNeeded = kvp.Value;
                var currentStock = await readRepository.GetStockQuantityByVariantIdAsync(
                    variantId,
                    colorId,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (currentStock < totalNeeded)
                {
                    var colorSuffix = colorId.HasValue ? $" (Màu ID {colorId})" : string.Empty;
                    errors.Add(
                        Error.BadRequest(
                            $"Sản phẩm ID {variantId}{colorSuffix} không đủ tồn kho. Tổng kho hiện có: {currentStock}, Tổng đơn hàng cần: {totalNeeded}, Thiếu: {totalNeeded - currentStock}",
                            "Products"));
                }
            }
        }
        if (errors.Count > 0)
        {
            return errors;
        }
        foreach (var output in outputsList)
        {
            if (isCompleting)
            {
                var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, true, cancellationToken)
                    .ConfigureAwait(false);
                if (result.IsFailure)
                    return result.Errors!;
            } else if (string.Compare(request.StatusId, OrderStatus.Delivering) == 0)
            {
                var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, false, cancellationToken)
                    .ConfigureAwait(false);
                if (result.IsFailure)
                    return result.Errors!;
            }
            output.StatusId = request.StatusId;
            updateRepository.Update(output);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (isCompleting)
        {
            foreach (var output in outputsList)
            {
                await commissionUpdateRepository.CalculateAndRecordCommissionAsync(output.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        } else if (string.Compare(request.StatusId, OrderStatus.Cancelled) == 0 ||
            string.Compare(request.StatusId, OrderStatus.Refunded) == 0)
        {
            foreach (var output in outputsList)
            {
                await commissionUpdateRepository.VoidCommissionAsync(output.Id, cancellationToken).ConfigureAwait(false);
            }
        }
        return outputsList.Adapt<List<OutputItemResponse>>();
    }
}
