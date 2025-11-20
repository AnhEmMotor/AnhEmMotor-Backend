using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier;

public class SupplierDeleteService(
    ISupplierSelectRepository supplierSelectRepository,
    ISupplierDeleteRepository supplierDeleteRepository,
    IUnitOfWork unitOfWork) : ISupplierDeleteService
{
    public async Task<ErrorResponse?> DeleteSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var supplier = await supplierSelectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {id} not found." }]
            };
        }

        supplierDeleteRepository.Delete(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();

        var activeSuppliers = await supplierSelectRepository.GetActiveSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSuppliersMap = allSuppliers.ToDictionary(s => s.Id);
        var activeSuppliersSet = activeSuppliers.Select(s => s.Id).ToHashSet();

        var errorDetails = new List<ErrorDetail>();

        foreach (var id in uniqueIds)
        {
            if (!allSuppliersMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Supplier not found",
                    Field = $"Supplier ID: {id}"
                });
                continue;
            }

            if (!activeSuppliersSet.Contains(id))
            {
                var supplierName = allSuppliersMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Supplier has already been deleted",
                    Field = supplierName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeSuppliers.Count > 0)
        {
            supplierDeleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}