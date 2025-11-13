using Application.ApiContracts.Supplier;
using Domain.Helpers;
using Sieve.Models;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierSelectService
    {
        Task<SupplierResponse?> GetSupplierByIdAsync(int id);
        Task<PagedResult<SupplierResponse>> GetSuppliersAsync(SieveModel sieveModel);
        Task<PagedResult<SupplierResponse>> GetDeletedSuppliersAsync(SieveModel sieveModel);
    }
}
