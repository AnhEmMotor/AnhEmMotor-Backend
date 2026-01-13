using Domain.Entities;

namespace Domain.DomainServices;

public class InventoryValuationService
{
    public static long CalculateUnitCostAndDeductInventory(List<InputInfo> availableBatches, int quantityToSell)
    {
        decimal totalCost = 0;
        var quantityNeeded = quantityToSell;

        foreach(var batch in availableBatches)
        {
            if(quantityNeeded <= 0)
                break;

            var batchRemaining = batch.RemainingCount ?? 0;
            var batchPrice = batch.InputPrice ?? 0;

            if(batchRemaining >= quantityNeeded)
            {
                totalCost += quantityNeeded * batchPrice;
                batch.RemainingCount = batchRemaining - quantityNeeded;
                quantityNeeded = 0;
                break;
            }

            totalCost += batchRemaining * batchPrice;
            quantityNeeded -= (short)batchRemaining;
            batch.RemainingCount = 0;
        }

        if(quantityNeeded > 0)
        {
            return -1;
        }

        if(quantityToSell == 0)
            return 0;

        return (long)Math.Round(totalCost / quantityToSell);
    }
}