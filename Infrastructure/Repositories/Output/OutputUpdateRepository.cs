using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OutputEntity = Domain.Entities.Output;
using OutputInfoEntity = Domain.Entities.OutputInfo;

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

    public async Task ProcessCOGSForCompletedOrderAsync(
        int outputId,
        CancellationToken cancellationToken)
    {
        var outputInfos = await context.OutputInfos
            .Where(oi => oi.OutputId == outputId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach(var outputInfo in outputInfos)
        {
            if(outputInfo.ProductId is null || outputInfo.Count is null)
            {
                continue;
            }

            var costPrice = await CalculateCOGSFIFOAsync(
                outputInfo.ProductId.Value,
                outputInfo.Count.Value,
                cancellationToken)
                .ConfigureAwait(false);

            outputInfo.CostPrice = costPrice;
        }
    }

    private async Task<long> CalculateCOGSFIFOAsync(
        int variantId,
        short quantityToSell,
        CancellationToken cancellationToken)
    {
        long totalCost = 0;
        var quantityNeeded = quantityToSell;

        var batches = await context.InputInfos
            .Where(ii => ii.ProductId == variantId && ii.RemainingCount > 0)
            .OrderBy(ii => ii.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach(var batch in batches)
        {
            if(quantityNeeded <= 0)
            {
                break;
            }

            var batchRemaining = batch.RemainingCount ?? 0;
            var batchPrice = batch.InputPrice ?? 0;

            if(batchRemaining >= quantityNeeded)
            {
                totalCost += quantityNeeded * batchPrice;
                batch.RemainingCount = batchRemaining - quantityNeeded;
                quantityNeeded = 0;
                break;
            }
            else
            {
                totalCost += batchRemaining * batchPrice;
                quantityNeeded -= (short)batchRemaining;
                batch.RemainingCount = 0;
            }
        }

        if(quantityNeeded > 0)
        {
            throw new InvalidOperationException(
                $"Không đủ hàng tồn kho. Chỉ có thể bán {quantityToSell - quantityNeeded}, thiếu {quantityNeeded} sản phẩm.");
        }

        return (long)Math.Round((decimal)totalCost / quantityToSell);
    }
}
