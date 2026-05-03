using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputUpdateRepository(ApplicationDBContext context) : IOutputUpdateRepository
{
    public void Update(OutputEntity output)
    {
        context.OutputOrders.Update(output);
    }

    public void Restore(OutputEntity output)
    {
        context.Restore(output);
    }

    public void Restore(IEnumerable<OutputEntity> outputs)
    {
        context.RestoreDeleteUsingSetColumnRange(outputs);
    }

    public async Task<Result<bool>> HandleInventoryTransactionAsync(
        int outputId,
        bool commitDeduction,
        CancellationToken cancellationToken)
    {
        var outputInfos = await context.OutputInfos
            .Include(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .Where(oi => oi.OutputId == outputId && oi.DeletedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var errors = new List<Error>();
        var transactions = new List<(OutputInfo Info, List<InputInfo> Batches, int Quantity)>();
        foreach (var outputInfo in outputInfos)
        {
            if (outputInfo.ProductVarientId is null || outputInfo.Count is null || outputInfo.Count <= 0)
            {
                continue;
            }
            var batches = await GetAvailableBatchesAsync(outputInfo.ProductVarientId.Value, cancellationToken)
                .ConfigureAwait(false);
            var totalAvailable = batches.Sum(b => b.RemainingCount ?? 0);
            if (totalAvailable < outputInfo.Count.Value)
            {
                var productName = outputInfo.ProductVariant?.Product?.Name ?? $"ID {outputInfo.ProductVarientId}";
                errors.Add(
                    Error.BadRequest(
                        $"Sản phẩm '{productName}' không đủ tồn kho. Hiện có: {totalAvailable}, cần: {outputInfo.Count.Value}",
                        "StatusId"));
            } else if (commitDeduction)
            {
                transactions.Add((outputInfo, batches, outputInfo.Count.Value));
            }
        }
        if (errors.Count > 0)
        {
            return Result<bool>.Failure(errors);
        }
        if (commitDeduction)
        {
            foreach (var (info, batches, quantityNeeded) in transactions)
            {
                decimal totalCost = 0;
                var remainingToDeduct = quantityNeeded;
                foreach (var batch in batches)
                {
                    if (remainingToDeduct <= 0)
                        break;
                    var batchRemaining = batch.RemainingCount ?? 0;
                    var batchPrice = batch.InputPrice ?? 0;
                    if (batchRemaining >= remainingToDeduct)
                    {
                        totalCost += remainingToDeduct * batchPrice;
                        batch.RemainingCount = batchRemaining - remainingToDeduct;
                        remainingToDeduct = 0;
                    } else
                    {
                        totalCost += batchRemaining * batchPrice;
                        remainingToDeduct -= batchRemaining;
                        batch.RemainingCount = 0;
                    }
                }
                info.CostPrice = Math.Round(totalCost / quantityNeeded, 2);
            }
        }
        return true;
    }

    private Task<List<InputInfo>> GetAvailableBatchesAsync(int productId, CancellationToken cancellationToken)
    {
        var finishedStatuses = Domain.Constants.Input.InputStatus.FinishInputValues;
        return context.InputInfos
            .Include(ii => ii.InputReceipt)
            .Where(
                ii => ii.ProductId == productId &&
                    ii.RemainingCount > 0 &&
                    ii.DeletedAt == null &&
                    ii.InputReceipt != null &&
                    ii.InputReceipt.DeletedAt == null &&
                    finishedStatuses.Contains(ii.InputReceipt.StatusId))
            .OrderBy(ii => ii.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}