using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Helpers;

namespace Application.Services.Supplier
{
    public class SupplierUpdateService(ISupplierSelectRepository supplierSelectRepository, ISupplierUpdateRepository supplierUpdateRepository) : ISupplierUpdateService
    {
        public async Task<bool> UpdateSupplierAsync(int id, UpdateSupplierRequest request)
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id);

            if (supplier == null) return false;

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

            await supplierUpdateRepository.UpdateSupplierAsync(supplier);
            return true;
        }

        public async Task<bool> UpdateSupplierStatusAsync(int id, UpdateSupplierStatusRequest request)
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id);

            if (supplier == null) return false;

            supplier.StatusId = request.StatusId;

            await supplierUpdateRepository.UpdateSupplierAsync(supplier);
            return true;
        }

        public async Task<ErrorResponse?> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request)
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

                if (supplier is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = "Supplier ID: " + id.ToString()
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
                await supplierUpdateRepository.UpdateSuppliersAsync(activeSuppliers);
            }

            return null;
        }

        public async Task<bool> RestoreSupplierAsync(int id)
        {
            var supplierList = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync([id]);

            if (supplierList.Count == 0) return false;

            await supplierUpdateRepository.RestoreSupplierAsync(supplierList[0]);
            return true;
        }

        public async Task<ErrorResponse?> RestoreSuppliersAsync(RestoreManySuppliersRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var deletedSuppliers = await supplierSelectRepository.GetDeletedSuppliersByIdsAsync(request.Ids);
            var allSuppliers = await supplierSelectRepository.GetAllSuppliersByIdsAsync(request.Ids);

            foreach (var id in request.Ids)
            {
                var supplier = allSuppliers.FirstOrDefault(s => s.Id == id);
                var deletedSupplier = deletedSuppliers.FirstOrDefault(s => s.Id == id);

                if (supplier == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Supplier not found",
                        Field = "Supplier ID: " + id.ToString()
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
                await supplierUpdateRepository.RestoreSuppliersAsync(deletedSuppliers);
            }

            return null;
        }
    }
}
