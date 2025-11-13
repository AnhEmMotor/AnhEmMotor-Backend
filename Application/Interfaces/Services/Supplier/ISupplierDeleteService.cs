using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierDeleteService
    {
        Task<bool> DeleteSupplierAsync(int id);
        Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request);
    }
}
