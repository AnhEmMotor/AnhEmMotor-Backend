using Application.ApiContracts.Supplier;
using Domain.Helpers;
using Sieve.Models;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierSelectService
    {
        Task<(SupplierResponse? Data, ErrorResponse? Error)> GetSupplierByIdAsync(int id, CancellationToken cancellationToken);
        Task<PagedResult<SupplierResponse>> GetSuppliersAsync(SieveModel sieveModel, CancellationToken cancellationToken);
        Task<PagedResult<SupplierResponse>> GetDeletedSuppliersAsync(SieveModel sieveModel, CancellationToken cancellationToken);
    }
}
