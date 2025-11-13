using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierUpdateService
    {
        Task<bool> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
        Task<bool> UpdateSupplierStatusAsync(int id, UpdateSupplierStatusRequest request);
        Task<ErrorResponse?> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request);
        Task<bool> RestoreSupplierAsync(int id);
        Task<ErrorResponse?> RestoreSuppliersAsync(RestoreManySuppliersRequest request);
    }
}
