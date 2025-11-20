using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier;

public class SupplierUpdateService(
    ISupplierSelectRepository supplierSelectRepository,
    ISupplierUpdateRepository supplierUpdateRepository,
    IUnitOfWork unitOfWork) : ISupplierUpdateService
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> UpdateSupplierAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierSelectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {id} not found." }]
            });
        }

        if (request.Name is not null) supplier.Name = request.Name;
        if (request.Phone is not null) supplier.Phone = request.Phone;
        if (request.Email is not null) supplier.Email = request.Email;
        if (request.StatusId is not null) supplier.StatusId = request.StatusId;
        if (request.Notes is not null) supplier.Notes = request.Notes;
        if (request.Address is not null) supplier.Address = request.Address;

        supplierUpdateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            StatusId = supplier.StatusId,
            Notes = supplier.Notes,
            Address = supplier.Address
        }, null);
    }

    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> UpdateSupplierStatusAsync(int id, UpdateSupplierStatusRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierSelectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {id} not found." }]
            });
        }

        supplier.StatusId = request.StatusId;

        supplierUpdateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            StatusId = supplier.StatusId,
            Notes = supplier.Notes,
            Address = supplier.Address
        }, null);
    }

    public async Task<(List<int>? Data, ErrorResponse? Error)> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
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
                    Field = $"Supplier ID: {id}",
                });
                continue;
            }

            if (!activeSuppliersSet.Contains(id))
            {
                var supplierName = allSuppliersMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Supplier has been deleted",
                    Field = supplierName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        foreach (var supplierToUpdate in activeSuppliers)
        {
            supplierToUpdate.StatusId = request.StatusId;
        }

        if (activeSuppliers.Count > 0)
        {
            supplierUpdateRepository.Update(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (uniqueIds, null);
    }

    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> RestoreSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var supplierList = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync([id], cancellationToken).ConfigureAwait(false);

        if (supplierList.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Deleted supplier with Id {id} not found." }]
            });
        }

        var supplier = supplierList[0];
        supplierUpdateRepository.Restore(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            StatusId = supplier.StatusId,
            Notes = supplier.Notes,
            Address = supplier.Address
        }, null);
    }

    public async Task<(List<int>? Data, ErrorResponse? Error)> RestoreSuppliersAsync(RestoreManySuppliersRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();

        var deletedSuppliers = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSuppliersMap = allSuppliers.ToDictionary(s => s.Id);
        var deletedSuppliersSet = deletedSuppliers.Select(s => s.Id).ToHashSet();

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

            if (!deletedSuppliersSet.Contains(id))
            {
                var supplierName = allSuppliersMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Supplier is not deleted",
                    Field = supplierName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if (deletedSuppliers.Count > 0)
        {
            supplierUpdateRepository.Restore(deletedSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (uniqueIds, null);
    }
}