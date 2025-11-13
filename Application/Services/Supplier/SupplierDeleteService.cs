using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier
{
    public class SupplierDeleteService(ISupplierSelectRepository supplierSelectRepository, ISupplierDeleteRepository supplierDeleteRepository) : ISupplierDeleteService
    {
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id);

            if (supplier == null) return false;

            await supplierDeleteRepository.DeleteSupplierAsync(supplier);
            return true;
        }

        public async Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var activeSuppliers = await supplierSelectRepository.GetActiveSuppliersByIdsAsync(request.Ids);
            var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(request.Ids);

            foreach (var id in request.Ids)
            {
                var supplier = allSuppliers.FirstOrDefault(s => s.Id == id);
                var activeSupplier = activeSuppliers.FirstOrDefault(s => s.Id == id);

                if (supplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = "Supplier ID: " + id.ToString()
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
                await supplierDeleteRepository.DeleteSuppliersAsync(activeSuppliers);
            }

            return null;
        }

    }
}
