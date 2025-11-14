using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierUpdateService
    {
        Task<ErrorResponse?> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
        Task<ErrorResponse?> UpdateSupplierStatusAsync(
            int id,
            UpdateSupplierStatusRequest request
        );
        Task<ErrorResponse?> RestoreSupplierAsync(int id);
        Task<ErrorResponse?> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request);
        Task<ErrorResponse?> RestoreSuppliersAsync(RestoreManySuppliersRequest request);
    }
}
