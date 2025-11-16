using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierDeleteService
    {
        Task<ErrorResponse?> DeleteSupplierAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request, CancellationToken cancellationToken);
    }
}
