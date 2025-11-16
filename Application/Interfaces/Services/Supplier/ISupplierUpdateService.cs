using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierUpdateService
    {
        Task<ErrorResponse?> UpdateSupplierAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken);
        Task<ErrorResponse?> UpdateSupplierStatusAsync(
            int id,
            UpdateSupplierStatusRequest request,
            CancellationToken cancellationToken
        );
        Task<ErrorResponse?> RestoreSupplierAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request, CancellationToken cancellationToken);
        Task<ErrorResponse?> RestoreSuppliersAsync(RestoreManySuppliersRequest request, CancellationToken cancellationToken);
    }
}
