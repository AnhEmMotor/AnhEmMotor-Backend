using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier
{
    public class SupplierDeleteService(ISupplierSelectRepository supplierSelectRepository, ISupplierDeleteRepository supplierDeleteRepository) : ISupplierDeleteService
    {
        public async Task<ErrorResponse?> DeleteSupplierAsync(int id, CancellationToken cancellationToken)
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

            await supplierDeleteRepository.DeleteSupplierAsync(supplier, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request, CancellationToken cancellationToken)
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

                if (supplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = $"Supplier ID: {id}",
                    });
                }
                else if (activeSupplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier has already been deleted",
                        Field = supplier.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (activeSuppliers.Count > 0)
            {
                await supplierDeleteRepository.DeleteSuppliersAsync(activeSuppliers, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

    }
}
