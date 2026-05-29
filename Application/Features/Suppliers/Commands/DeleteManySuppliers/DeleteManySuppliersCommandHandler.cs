using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IInventoryReceiptReadRepository InventoryReceiptReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManySuppliersCommand, Result>
{
    public async Task<Result> Handle(DeleteManySuppliersCommand request, CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();
        var allSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var activeSupplierSet = activeSuppliers.Select(s => s.Id).ToHashSet();
        var relevantInventoryReceipts = await InventoryReceiptReadRepository.GetBySupplierIdsAsync(
            uniqueIds,
            cancellationToken)
            .ConfigureAwait(false);
        var suppliersWithWorkingInventoryReceiptsSet = relevantInventoryReceipts
            .Where(x => InventoryReceiptStatus.IsCanEdit(x.StatusId))
            .SelectMany(
                x => x.InventoryReceiptInfos
                    .Where(
                        ii => ii.QuotationProductRow != null &&
                                ii.QuotationProductRow.QuotationReceipt != null &&
                                ii.QuotationProductRow.QuotationReceipt.SupplierId.HasValue)
                    .Select(ii => ii.QuotationProductRow!.QuotationReceipt!.SupplierId!.Value))
            .Intersect(uniqueIds)
            .ToHashSet();
        foreach (var id in uniqueIds)
        {
            if (!allSupplierMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Supplier with Id {id} not found.", "Id"));
            } else if (!activeSupplierSet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Supplier with Id {id} has already been deleted.", "Id"));
            } else if (suppliersWithWorkingInventoryReceiptsSet.Contains(id))
            {
                errorDetails.Add(
                    Error.BadRequest(
                        $"Supplier with Id {id} cannot be deleted because it has working InventoryReceipt receipts.",
                        "Id"));
            }
        }
        if (errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }
        if (activeSuppliers.ToList().Count > 0)
        {
            deleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        return Result.Success();
    }
}
