using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierDeleteService
    {
        Task<ErrorResponse?> DeleteSupplierAsync(int id);
        Task<ErrorResponse?> DeleteSuppliersAsync(DeleteManySuppliersRequest request);
    }
}
