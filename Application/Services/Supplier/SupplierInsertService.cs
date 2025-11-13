using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Services.Supplier
{
    public class SupplierInsertService(ISupplierInsertRepository supplierInsertRepository) : ISupplierInsertService
    {
        public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request)
        {
            var supplier = new SupplierEntity
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                StatusId = request.StatusId,
                Notes = request.Notes,
                Address = request.Address
            };

            var createdSupplier = await supplierInsertRepository.AddSupplierAsync(supplier);

            return new SupplierResponse
            {
                Id = createdSupplier.Id,
                Name = createdSupplier.Name,
                Phone = createdSupplier.Phone,
                Email = createdSupplier.Email,
                StatusId = createdSupplier.StatusId,
                Notes = createdSupplier.Notes,
                Address = createdSupplier.Address
            };
        }
    }
}
