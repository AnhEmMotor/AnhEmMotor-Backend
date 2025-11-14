using Application.ApiContracts.Brand;
using Domain.Helpers;
using Sieve.Models;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandSelectService
    {
        Task<(BrandResponse? Data, ErrorResponse? Error)> GetBrandByIdAsync(int id);
        Task<PagedResult<BrandResponse>> GetBrandsAsync(SieveModel sieveModel);
        Task<PagedResult<BrandResponse>> GetDeletedBrandsAsync(SieveModel sieveModel);
    }
}
