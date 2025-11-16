using Application.ApiContracts.Supplier;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierInsertService
    {
        Task<SupplierResponse> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
    }
}
