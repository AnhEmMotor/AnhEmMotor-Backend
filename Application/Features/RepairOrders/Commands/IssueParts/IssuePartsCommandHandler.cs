using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.Service;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RepairOrders.Commands.IssueParts
{
    public class IssuePartsCommandHandler(
        IRepairOrderReadRepository repairOrderReadRepository,
        IRepairOrderUpdateRepository repairOrderUpdateRepository,
        IServiceReadRepository serviceReadRepository,
        IProductVariantReadRepository productVariantReadRepository,
        IInventoryReceiptInfoReadRepository inventoryReceiptInfoReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<IssuePartsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(IssuePartsCommand request, CancellationToken cancellationToken)
        {
            var repairOrder = await repairOrderReadRepository.GetByIdAsync(request.RepairOrderId, cancellationToken)
                .ConfigureAwait(false);

            if (repairOrder == null)
            {
                return Result<bool>.Failure(Error.NotFound("Phiếu sửa chữa không tồn tại."));
            }

            // 1. Revert existing parts inventory deductions
            var existingParts = repairOrder.Details.Where(d => d.Type == "Part").ToList();
            foreach (var detail in existingParts)
            {
                if (detail.ProductVariantId == null) continue;

                var inputInfos = await inventoryReceiptInfoReadRepository.GetFinishedInventoryReceiptInfosByVariantIdAsync(detail.ProductVariantId.Value, cancellationToken)
                    .ConfigureAwait(false);
                inputInfos = inputInfos.OrderBy(ii => ii.Id).ToList();

                var remainingToRevert = detail.Count;
                foreach (var ii in inputInfos)
                {
                    if (remainingToRevert <= 0) break;
                    var maxCapacity = (ii.Count ?? 0) - (ii.RemainingCount ?? 0);
                    if (maxCapacity > 0)
                    {
                        var addAmount = Math.Min(remainingToRevert, maxCapacity);
                        ii.RemainingCount = (ii.RemainingCount ?? 0) + addAmount;
                        remainingToRevert -= addAmount;
                    }
                }

                if (remainingToRevert > 0 && inputInfos.Count > 0)
                {
                    inputInfos[0].RemainingCount = (inputInfos[0].RemainingCount ?? 0) + remainingToRevert;
                }
            }

            // 2. Clear old details
            if (repairOrder.Details.Any())
            {
                repairOrderUpdateRepository.RemoveDetailsRange(repairOrder.Details);
                repairOrder.Details.Clear();
            }

            decimal laborCost = 0;
            decimal partsCost = 0;

            // 3. Process new Services
            foreach (var serviceItem in request.Services)
            {
                var serviceExists = await serviceReadRepository.ExistsAsync(serviceItem.ServiceId, cancellationToken)
                    .ConfigureAwait(false);
                if (!serviceExists)
                {
                    return Result<bool>.Failure(Error.NotFound($"Dịch vụ ID {serviceItem.ServiceId} không tồn tại."));
                }

                var detail = new RepairOrderDetail
                {
                    RepairOrderId = repairOrder.Id,
                    ServiceId = serviceItem.ServiceId,
                    ProductVariantId = null,
                    Count = 1,
                    Price = 0,
                    LaborCost = serviceItem.LaborCost,
                    Type = "Service",
                    Notes = serviceItem.Notes
                };
                laborCost += serviceItem.LaborCost;
                repairOrderUpdateRepository.AddDetail(detail);
            }

            // 4. Process new Parts (FIFO inventory deduction)
            foreach (var partItem in request.Parts)
            {
                var variant = await productVariantReadRepository.GetByIdWithDetailsAsync(partItem.ProductVariantId, cancellationToken)
                    .ConfigureAwait(false);
                if (variant == null)
                {
                    return Result<bool>.Failure(Error.NotFound($"Phụ tùng (Biến thể sản phẩm) ID {partItem.ProductVariantId} không tồn tại."));
                }

                // Query available finished input receipt items for this variant
                var allInputInfos = await inventoryReceiptInfoReadRepository.GetFinishedInventoryReceiptInfosByVariantIdAsync(partItem.ProductVariantId, cancellationToken)
                    .ConfigureAwait(false);
                
                var inputInfos = allInputInfos
                    .Where(ii => (ii.RemainingCount ?? 0) > 0)
                    .OrderBy(ii => ii.InventoryReceipt != null ? ii.InventoryReceipt.InventoryReceiptDate : DateTimeOffset.MinValue)
                    .ToList();

                var availableStock = inputInfos.Sum(ii => ii.RemainingCount ?? 0);
                if (availableStock < partItem.Count)
                {
                    return Result<bool>.Failure(Error.BadRequest($"Không đủ hàng trong kho cho phụ tùng: {variant.VariantName}. Còn lại: {availableStock}, Yêu cầu: {partItem.Count}"));
                }

                var remainingToDeduct = partItem.Count;
                foreach (var ii in inputInfos)
                {
                    if (remainingToDeduct <= 0) break;
                    var availableInThisInput = ii.RemainingCount ?? 0;
                    if (availableInThisInput > 0)
                    {
                        var deductAmount = Math.Min(remainingToDeduct, availableInThisInput);
                        ii.RemainingCount = availableInThisInput - deductAmount;
                        remainingToDeduct -= deductAmount;
                    }
                }

                var detail = new RepairOrderDetail
                {
                    RepairOrderId = repairOrder.Id,
                    ServiceId = null,
                    ProductVariantId = partItem.ProductVariantId,
                    Count = partItem.Count,
                    Price = partItem.Price,
                    LaborCost = 0,
                    Type = "Part",
                    Notes = partItem.Notes
                };
                partsCost += partItem.Count * partItem.Price;
                repairOrderUpdateRepository.AddDetail(detail);
            }

            // 5. Update RepairOrder stats
            repairOrder.LaborCost = laborCost;
            repairOrder.PartsCost = partsCost;
            repairOrder.TotalAmount = laborCost + partsCost;

            if (!string.IsNullOrEmpty(request.Status))
            {
                repairOrder.Status = request.Status;
            }
            else if (repairOrder.Status == "Pending")
            {
                repairOrder.Status = "InProgress";
            }

            repairOrderUpdateRepository.Update(repairOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
