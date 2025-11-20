using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Services.Supplier
{
    public class SupplierInsertService(ISupplierInsertRepository supplierInsertRepository, IUnitOfWork unitOfWork) : ISupplierInsertService
    {
        public async Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
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

            await supplierInsertRepository.AddAsync(supplier, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new SupplierResponse
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                Email = supplier.Email,
                StatusId = supplier.StatusId,
                Notes = supplier.Notes,
                Address = supplier.Address
            };
        }
    }
}
