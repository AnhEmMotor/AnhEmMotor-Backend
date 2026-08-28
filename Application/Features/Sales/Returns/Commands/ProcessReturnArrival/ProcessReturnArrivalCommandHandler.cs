using Application.Common.Models;
using Application.Features.InventoryOnHand.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Constants.InventoryReceipt;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnArrival;

public class ProcessReturnArrivalCommandHandler(
    IReturnRequestReadRepository readRepository,
    IReturnRequestWriteRepository writeRepository,
    IInventoryReceiptInsertRepository receiptInsertRepository,
    IUnitOfWork unitOfWork,
    IInventoryReceiptReadRepository? receiptReadRepository = null,
    IInventoryReceiptUpdateRepository? receiptUpdateRepository = null,
    IOutputReadRepository? outputReadRepository = null,
    IInventoryLedgerRepository? ledgerRepository = null,
    IPublisher? publisher = null) : IRequestHandler<ProcessReturnArrivalCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ProcessReturnArrivalCommand request, CancellationToken cancellationToken)
    {
        var returnRequests = await readRepository
            .GetByOrderIdAsync(request.OutputId, cancellationToken)
            .ConfigureAwait(false);

        var activeOrCompletedReturns = returnRequests
            .Where(x => x.Status != "rejected")
            .Where(x => !request.ReturnRequestId.HasValue || x.Id == request.ReturnRequestId.Value)
            .Where(x => string.IsNullOrEmpty(request.TrackingNumber) || x.OriginalTrackingNumber == request.TrackingNumber)
            .ToList();

        if (activeOrCompletedReturns.Count == 0)
        {
            return Result<int>.Success(0);
        }

        var output = outputReadRepository != null
            ? await outputReadRepository.GetByIdWithDetailsAsync(request.OutputId, cancellationToken, Domain.Constants.DataFetchMode.All).ConfigureAwait(false)
            : null;

        var existingReceipts = receiptReadRepository != null
            ? await receiptReadRepository.GetBySourceOrderIdAsync(request.OutputId, cancellationToken, Domain.Constants.DataFetchMode.All).ConfigureAwait(false)
            : new List<InventoryReceipt>();

        int processedCount = 0;
        var combosToUpdate = new HashSet<(int VariantId, int? ColorId)>();

        foreach (var returnRequest in activeOrCompletedReturns)
        {
            if (returnRequest.Status != "completed")
            {
                returnRequest.Status = "completed";
                returnRequest.ReturnAction ??= "restock";
                await writeRepository.UpdateAsync(returnRequest, cancellationToken).ConfigureAwait(false);
            }

            if (returnRequest.ReturnAction == "restock")
            {
                var existingReceipt = existingReceipts.FirstOrDefault(r =>
                    r.Notes != null && r.Notes.Contains($"#{returnRequest.Id}"))
                    ?? (activeOrCompletedReturns.Count == 1 ? existingReceipts.FirstOrDefault(r => r.StatusId == Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent) : null);

                if (existingReceipt != null)
                {
                    existingReceipt.StatusId = Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve;
                    if (receiptUpdateRepository != null)
                    {
                        receiptUpdateRepository.Update(existingReceipt);
                    }
                }
                else
                {
                    var receipt = new InventoryReceipt
                    {
                        InventoryReceiptDate = DateTimeOffset.UtcNow,
                        Notes = $"Restock from Return Request #{returnRequest.Id}",
                        StatusId = Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve,
                        SourceOrderId = returnRequest.OrderId,
                        InventoryReceiptInfos = returnRequest.Items.Select(i =>
                        {
                            var matchingInfo = output?.OutputInfos.FirstOrDefault(oi =>
                                (i.ProductVariantId.HasValue && oi.ProductVariantId == i.ProductVariantId && (!i.ProductVariantColorId.HasValue || oi.ProductVariantColorId == i.ProductVariantColorId)) ||
                                (oi.ProductVariant != null && oi.ProductVariant.ProductId == i.ProductId));

                            return new InventoryReceiptInfo
                            {
                                ParentOutputInfoId = matchingInfo?.Id,
                                Count = i.Quantity,
                                RemainingCount = i.Quantity
                            };
                        }).ToList()
                    };
                    receiptInsertRepository.Add(receipt);
                }

                // Ghi nhận vào sổ kho Ledger và cập nhật tồn kho OnHand
                foreach (var item in returnRequest.Items)
                {
                    var matchingInfo = output?.OutputInfos.FirstOrDefault(oi =>
                        (item.ProductVariantId.HasValue && oi.ProductVariantId == item.ProductVariantId && (!item.ProductVariantColorId.HasValue || oi.ProductVariantColorId == item.ProductVariantColorId)) ||
                        (oi.ProductVariant != null && oi.ProductVariant.ProductId == item.ProductId));

                    var variantId = item.ProductVariantId ?? matchingInfo?.ProductVariantId;
                    var colorId = item.ProductVariantColorId ?? matchingInfo?.ProductVariantColorId;

                    if (variantId.HasValue && ledgerRepository != null)
                    {
                        var lastEntry = await ledgerRepository.GetLastEntryAsync(variantId.Value, colorId, cancellationToken)
                            .ConfigureAwait(false);
                        var currentStock = lastEntry?.StockAfter ?? 0;
                        var importQty = item.Quantity;
                        var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : (matchingInfo?.Price ?? 0);

                        var ledger = new InventoryLedger
                        {
                            TransactionDate = DateTimeOffset.UtcNow,
                            DocumentCode = $"RET-{returnRequest.Id}",
                            TransactionType = "Nhập kho hoàn hàng",
                            ProductVariantId = variantId.Value,
                            ProductVariantColorId = colorId,
                            PartnerName = returnRequest.CustomerName,
                            ImportQty = importQty,
                            ExportQty = 0,
                            UnitPrice = unitPrice,
                            TotalAmount = importQty * unitPrice,
                            StockAfter = currentStock + importQty
                        };
                        await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
                        combosToUpdate.Add((variantId.Value, colorId));
                    }
                }

                processedCount++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (publisher != null && combosToUpdate.Count > 0)
        {
            await publisher.Publish(new InventoryChangedNotification(combosToUpdate), cancellationToken)
                .ConfigureAwait(false);
        }

        return Result<int>.Success(processedCount);
    }
}
