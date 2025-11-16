using Application.ApiContracts.Brand;
using Domain.Helpers;
using Sieve.Models;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandSelectService
    {
        Task<(BrandResponse? Data, ErrorResponse? Error)> GetBrandByIdAsync(int id, CancellationToken cancellationToken);
        Task<PagedResult<BrandResponse>> GetBrandsAsync(SieveModel sieveModel, CancellationToken cancellationToken);
        Task<PagedResult<BrandResponse>> GetDeletedBrandsAsync(SieveModel sieveModel, CancellationToken cancellationToken);
    }
}
