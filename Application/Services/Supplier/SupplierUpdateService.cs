using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier
{
    public class SupplierUpdateService(ISupplierSelectRepository supplierSelectRepository, ISupplierUpdateRepository supplierUpdateRepository) : ISupplierUpdateService
    {
        public async Task<ErrorResponse?> UpdateSupplierAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken)
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (supplier == null)
            {
                return new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = $"Supplier with Id {id} not found." }
                    ]
                };
            }

            if (request.Name is not null)
                supplier.Name = request.Name;
            if (request.Phone is not null)
                supplier.Phone = request.Phone;
            if (request.Email is not null)
                supplier.Email = request.Email;
            if (request.StatusId is not null)
                supplier.StatusId = request.StatusId;
            if (request.Notes is not null)
                supplier.Notes = request.Notes;
            if (request.Address is not null)
                supplier.Address = request.Address;

            await supplierUpdateRepository.UpdateSupplierAsync(supplier, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> UpdateSupplierStatusAsync(
            int id,
            UpdateSupplierStatusRequest request,
            CancellationToken cancellationToken
        )
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (supplier == null)
            {
                return new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = $"Supplier with Id {id} not found." }
                    ]
                };
            }

            supplier.StatusId = request.StatusId;

            await supplierUpdateRepository.UpdateSupplierAsync(supplier, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var activeSuppliers = await supplierSelectRepository.GetActiveSuppliersByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
            var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var supplier = allSuppliers.FirstOrDefault(s => s.Id == id);
                var activeSupplier = activeSuppliers.FirstOrDefault(s => s.Id == id);

                if (supplier is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = $"Supplier ID: {id}",
                    });
                }
                else if (activeSupplier is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier has been deleted",
                        Field = supplier.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            foreach (var supplierToUpdate in activeSuppliers)
            {
                supplierToUpdate.StatusId = request.StatusId;
            }

            if (activeSuppliers.Count > 0)
            {
                await supplierUpdateRepository.UpdateSuppliersAsync(activeSuppliers, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        public async Task<ErrorResponse?> RestoreSupplierAsync(int id, CancellationToken cancellationToken)
        {
            var supplierList = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync([id], cancellationToken).ConfigureAwait(false);

            if (supplierList.Count == 0)
            {
                return new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = $"Deleted supplier with Id {id} not found." }
                    ]
                };
            }

            await supplierUpdateRepository.RestoreSupplierAsync(supplierList[0], cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> RestoreSuppliersAsync(RestoreManySuppliersRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var deletedSuppliers = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
            var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var supplier = allSuppliers.FirstOrDefault(s => s.Id == id);
                var deletedSupplier = deletedSuppliers.FirstOrDefault(s => s.Id == id);

                if (supplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = $"Supplier ID: {id}"
                    });
                }
                else if (deletedSupplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier has already been restored",
                        Field = supplier.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (deletedSuppliers.Count > 0)
            {
                await supplierUpdateRepository.RestoreSuppliersAsync(deletedSuppliers, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
