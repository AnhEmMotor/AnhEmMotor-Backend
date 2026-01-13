using Application.Interfaces.Repositories.Output;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputUpdateRepository(ApplicationDBContext context) : IOutputUpdateRepository
{
    public void Update(OutputEntity output) { context.OutputOrders.Update(output); }

    public void Restore(OutputEntity output) { context.Restore(output); }

    public void Restore(IEnumerable<OutputEntity> outputs) { context.RestoreDeleteUsingSetColumnRange(outputs); }

    public async Task ProcessCOGSForCompletedOrderAsync(int outputId, CancellationToken cancellationToken)
    {
        var outputInfos = await context.OutputInfos
            .Where(oi => oi.OutputId == outputId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach(var outputInfo in outputInfos)
        {
            if(outputInfo.ProductVarientId is null || outputInfo.Count is null)
            {
                continue;
            }

            var batches = await GetAvailableBatchesAsync(outputInfo.ProductVarientId.Value, cancellationToken)
                .ConfigureAwait(false);

            var unitCost = Domain.DomainServices.InventoryValuationService
                .CalculateUnitCostAndDeductInventory(batches, outputInfo.Count.Value);

            outputInfo.CostPrice = unitCost;
        }
    }

    private Task<List<InputInfo>> GetAvailableBatchesAsync(int productId, CancellationToken cancellationToken)
    {
        var finishedStatuses = Domain.Constants.Input.InputStatus.FinishInputValues;

        return context.InputInfos
            .Include(ii => ii.InputReceipt)
            .Where(
                ii => ii.ProductId == productId &&
                    ii.RemainingCount > 0 &&
                    ii.InputReceipt != null &&
                    finishedStatuses.Contains(ii.InputReceipt.StatusId))
            .OrderBy(ii => ii.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }
}